-- Drop this first, then create at the bottom since there are multiple columns being removed from it
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ProposalChecklist]') AND name = N'IX_ProposalChecklist_C1') 
	DROP INDEX [IX_ProposalChecklist_C1] ON [dbo].[ProposalChecklist]
GO

/*
	## START ##
	
	12/13/2016 [twilson3] - BOEJ-1634 Approval Workflow Database
*/

IF NOT EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'Proposal' AND
					C.name = 'WorkflowStatus' AND
					S.name = 'dbo'
				)
BEGIN
	-- Add column and set default value (of 'Not Started').
	ALTER TABLE [dbo].[Proposal]
		ADD [WorkflowStatus] [int] NOT NULL DEFAULT 0
	ALTER TABLE [dbo].[Proposal]
		ADD [WorkflowStatusLastUpdated] datetime2 NULL
	ALTER TABLE [dbo].[Proposal]
		ADD [LeadEstimatorSignedDT] datetime2 NULL
	ALTER TABLE [dbo].[Proposal]
		ADD [LeadEstimatorSignComment] varchar(1000) NULL
	ALTER TABLE [dbo].[Proposal]
		ADD [CoverSheetApproverSignedDT] datetime2 NULL
	ALTER TABLE [dbo].[Proposal]
		ADD [CoverSheetApproverSignComment] varchar(1000) NULL
	ALTER TABLE [dbo].[Proposal]
		ADD [PricingVerifierSignedDT] datetime2 NULL
	ALTER TABLE [dbo].[Proposal]
		ADD [PricingVerifierSignComment] varchar(1000) NULL
	ALTER TABLE [dbo].[Proposal]
		ADD [IndependentReviewerSignedDT] datetime2 NULL
	ALTER TABLE [dbo].[Proposal]
		ADD [IndependentReviewerSignComment] varchar(1000) NULL
	ALTER TABLE [dbo].[Proposal]
		ADD [LOBEstimatingLeadSignedDT] datetime2 NULL
	ALTER TABLE [dbo].[Proposal]
		ADD [LOBEstimatingLeadSignComment] varchar(1000) NULL
	
END
GO

/*
	11/13/2016 [twilson3] - BOEJ:1634 Approval Workflow Database

	## END ##
*/

/*
	## START ##
	
	1/3/2017 [Tom] - insert/update statements for the new & changed users in PTM
*/

IF EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'ProposalChecklist' AND
					C.name = 'PeerReviewerUserID' AND
					S.name = 'dbo'
				)
BEGIN
	ALTER TABLE [dbo].ProposalChecklist DROP CONSTRAINT [FK_ProposalChecklist_genTRACUser]
	ALTER TABLE [dbo].ProposalChecklist DROP COLUMN [PeerReviewerUserID]
END
GO

IF NOT EXISTS(SELECT 1 FROM  [dbo].[RoleLU] WHERE Role = 'Tech Lead')
	Insert into [dbo].[RoleLU] values ('Tech Lead');

IF NOT EXISTS(SELECT 1 FROM  [dbo].[RoleLU] WHERE Role = 'Proposal Mgr')
	Insert into [dbo].[RoleLU] values ('Proposal Mgr');

IF NOT EXISTS(SELECT 1 FROM  [dbo].[RoleLU] WHERE Role = 'Cover Sheet Approver')
	Insert into [dbo].[RoleLU] values ('Cover Sheet Approver');

IF NOT EXISTS(SELECT 1 FROM  [dbo].[RoleLU] WHERE Role = 'Pricing Verification')
	Insert into [dbo].[RoleLU] values ('Pricing Verification');

IF NOT EXISTS(SELECT 1 FROM  [dbo].[RoleLU] WHERE Role = 'LOB Estimating Lead/Mgr')
	Insert into [dbo].[RoleLU] values ('LOB Estimating Lead/Mgr');

GO

Update [dbo].[RoleLU] set Role = 'Cost Volume Lead' where RoleID = 2;
Update [dbo].[RoleLU] set Role = 'Lead Estimator' where RoleID = 3;
Update [dbo].[RoleLU] set Role = 'Backup Lead Estimator' where RoleID = 12;
Update [dbo].[RoleLU] set Role = 'Material Lead' where RoleID = 6;
Update [dbo].[RoleLU] set Role = 'Subcontract Lead' where RoleID = 7;
GO

/*
	1/3/2017 [Tom] - insert/update statements for the new & changed users in PTM

	## END ##
*/
/*
	## START ##
	
	12/21/2016 [twilson3] - BOEJ-1143 Approval Emailer -- remove old Email tables/stored procs
*/
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateEmailSent]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateEmailSent];
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProposalEmail]') AND type in (N'U'))
	ALTER TABLE [dbo].[ProposalEmail] DROP CONSTRAINT [FK_ProposalEmail_Proposal]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProposalEmail]') AND type in (N'U'))
	DROP TABLE [dbo].[ProposalEmail]
GO	

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getProposalEmail]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getProposalEmail];
GO

/*
	12/21/2016 [twilson3] - BOEJ-1143 Approval Emailer -- remove old Email tables/stored procs

	## END ##
*/ 

/*
	## START ##

	1/5/2017 [Dusan] - Removing revisions, LMIS and fixing some other stuff
*/ 

UPDATE [dbo].[RoleLU] SET Role = 'Contracts Lead' WHERE Role = 'Contracts POC';
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getPreviousProposalRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getPreviousProposalRevision];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[revertProposalRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[revertProposalRevision];
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertProposalRevision]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertProposalRevision];
GO

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'IsLMIS' AND Object_ID = Object_ID('[dbo].[Proposal]'))
BEGIN
	DROP INDEX IX_Proposal_C1 ON Proposal;
    ALTER TABLE [dbo].Proposal DROP COLUMN IsLMIS;
END
GO

IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'RevisionId' AND Object_ID = Object_ID('[dbo].[Proposal]'))
BEGIN
	DROP INDEX IX_Proposal_C2 ON Proposal;
	ALTER TABLE [dbo].Proposal DROP CONSTRAINT DF_Proposal_RevisionID;
    ALTER TABLE [dbo].Proposal DROP COLUMN RevisionId;
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[fk_ProposalId]') AND parent_object_id = OBJECT_ID(N'[dbo].[ProposalContractTypeXREF]'))
BEGIN
	ALTER TABLE [dbo].[ProposalContractTypeXREF] 
		ADD CONSTRAINT fk_ProposalId
		FOREIGN KEY (ProposalId) REFERENCES Proposal(ProposalId);
END
GO

/*
	1/5/2017 [Dusan] - Removing revisions, LMIS and fixing some other stuff

	## END ##
*/ 

/*
	## START ##
	
	1/5/2017 [twilson3] - BOEJ-1706 Remove ICE fields
*/
IF EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'ProposalChecklist' AND
					C.name = 'ICECostConfidence' AND
					S.name = 'dbo'
				)
BEGIN
	
	
	ALTER TABLE [dbo].[ProposalChecklist] DROP column [ICECostConfidence]
	
	ALTER TABLE [dbo].[ProposalChecklist] DROP CONSTRAINT [CK_ProposalChecklist_ICENominalROSPercentage]
		
	ALTER TABLE [dbo].[ProposalChecklist] DROP column [ICENominalROSPercentage]
END
GO

/*
	1/5/2017 [twilson3] - BOEJ-1706 Remove ICE fields

	## END ##
*/

/*
	## START ##

	1/9/2017 [Chris] - BOEJ-1682 - Remove "Segment" Field
*/ 

-- Update the Index

CREATE NONCLUSTERED INDEX [IX_ProposalChecklist_C1] ON [dbo].[ProposalChecklist]
       (
              [ProposalSubmittalDate] ASC,
              [ProposalID] ASC
       )
       INCLUDE (     [UpdateDate],
              [ISGSTotalPrice],
              [ProfitFee],
              [ROSPercentage],
              [LMLaborHours],
              [LMLaborCost],
              [SubcontractorCost],
              [MaterialCost],
              [IWTACost],
              [TravelCost],
              [OtherDirectCost]) WITH (PAD_INDEX = ON, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 80) ON [PRIMARY]
GO 


-- Drop the Foreign Key.
IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS WHERE CONSTRAINT_NAME ='FK_ProposalChecklist_SegmentLU')
	ALTER TABLE dbo.ProposalChecklist DROP CONSTRAINT FK_ProposalChecklist_SegmentLU;
GO

-- Drop the Column.
IF EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'SegmentId' AND Object_ID = Object_ID('[dbo].[ProposalChecklist]'))
	ALTER TABLE dbo.ProposalChecklist DROP COLUMN SegmentId;
GO

-- Drop the table.
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'SegmentLU')
	DROP TABLE dbo.SegmentLU;
GO

-- Drop the procedure.
IF EXISTS (select 1 from dbo.sysobjects where id = object_id(N'[dbo].[rsSegment]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
	DROP PROCEDURE [dbo].[rsSegment];
GO

/*
	1/9/2017 [Chris] - BOEJ-1682 - Remove "Segment" Field

	## END ##
*/ 

/*
	## START ##

	1/17/2017 [Joe] - BOEJ-1727 - Rename Pricer and Peer Reviewer
	1/17/2017 [Joe] - Removing data that is no longer being used and causing issues.
*/ 

--Rename pricer and peer reviewer in PPR Checklist
UPDATE [dbo].[PPRChecklistContent] 
	SET [ChecklistText] = '<p>&nbsp;<span style="font-style:italic; font-weight: bold;">To be completed by the Lead Estimator for all proposals regardless of proposal type, value, or waiver for completion of the Proposal Adequacy Review</span></p><br/>' 
	WHERE [ChecklistText] = '<p>&nbsp;<span style="font-style:italic; font-weight: bold;">To be completed by the Pricer for all proposals regardless of proposal type, value, or waiver for completion of the Proposal Adequacy Review</span></p><br/>';
UPDATE [dbo].[PPRChecklistContent] 
	SET [ChecklistText] = '<p><span style="font-style:italic; font-weight: bold;">Lead Estimator Comments (<span style="color: #0070c0">comments required for all “No” responses EXCEPT Question 1</span>)  <span style="color: red">All information must be unclassified and non-export controlled.</span></span></p>' 
	WHERE [ChecklistText] = '<p><span style="font-style:italic; font-weight: bold;">Pricer Comments (<span style="color: #0070c0">comments required for all “No” responses EXCEPT Question 1</span>)  <span style="color: red">All information must be unclassified and non-export controlled.</span></span></p>';
UPDATE [dbo].[PPRChecklistContent] 
	SET [ChecklistText] = '<p><span style="color: #002060; font-style:italic; font-weight: bold;">*Default is set to "No." The Proposal Adequacy Review Document will appear for Lead Estimator and Independent Reviewer. If changed to "Yes," the  Proposal Adequacy form is changed to inactive and any responses are removed.</span></p>' 
	WHERE [ChecklistText] = '<p><span style="color: #002060; font-style:italic; font-weight: bold;">*Default is set to "No." The Proposal Adequacy Review Document will appear for Pricer and Peer Reviewer. If changed to "Yes," the  Proposal Adequacy form is changed to inactive and any responses are removed.</span></p>';

--Rename pricer and peer reviewer in PAR Checklist
UPDATE [dbo].[PARChecklistContent] 
	SET [ChecklistText] = '<span style="font-style:italic; font-weight: bold;">To be completed by the Lead Estimator and Independent Reviewer for all Proposals that meet any of the following criteria:</span>' 
	WHERE [ChecklistText] = '<span style="font-style:italic; font-weight: bold;">To be completed by the Estimator and Peer reviewer for all Proposals that meet any of the following criteria:</span>';
UPDATE [dbo].[PARChecklistContent] 
	SET [ChecklistText] = '<p><span style="font-weight:bold; font-style:italic;">Lead Estimator Comments (<span style="color: #0070c0">comments required for all "No" responses</span>)  <span>All information must be unclassified and non-export controlled.</span></span></p>' 
	WHERE [ChecklistText] = '<p><span style="font-weight:bold; font-style:italic;">Estimator Comments (<span style="color: #0070c0">comments required for all "No" responses</span>)  <span style="color: red">All information must be unclassified and non-export controlled.</span></span></p>';

UPDATE dbo.ProposalUserRole SET RoleTypeID = NULL WHERE RoleID = 3

/*
	1/17/2017 [Joe] - BOEJ-1727 - Rename Pricer and Peer Reviewer
	1/17/2017 [Joe] - Removing data that is no longer being used and causing issues.

	## END ##
*/
 
/*
	1/16/2017 [twilson3] - BOEJ-1604 - Cleanup DB Project
*/
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateProposalLogReportByLatestVersionForGroove]') AND type in (N'V'))
	DROP VIEW [dbo].[CreateProposalLogReportByLatestVersionForGroove]
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateProposalLogReportByLatestVersionForGroove]') AND type in (N'V'))
	DROP VIEW [dbo].[CreateProposalLogReportByLatestVersionForGrooveToNow]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[generationMaintenance]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[generationMaintenance]
GO

IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[IndexFragmentation]') AND name = N'IX_IndexFragmentation__TableName_IndexName_FillFactor') 
	DROP INDEX [IX_IndexFragmentation__TableName_IndexName_FillFactor] ON [dbo].[IndexFragmentation] WITH ( ONLINE = OFF )

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'IndexFragmentation')
	DROP TABLE dbo.IndexFragmentation
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteSystemRolesByUserID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteSystemRolesByUserID]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateProposalChecklistVersion]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateProposalChecklistVersion]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[genBOE].[getProposalByProposalID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [genBOE].[getProposalByProposalID]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[genBOE].[getProposalByProposalTracking]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [genBOE].[getProposalByProposalTracking]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[genBOE].[getProposalBySearch]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [genBOE].[getProposalBySearch]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profit].[CleanUp]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [Profit].[CleanUp]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profit].[DataMigrationUpsertProposal]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [Profit].[DataMigrationUpsertProposal]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profit].[InsertLookUpData]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [Profit].[InsertLookUpData]
GO
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profit].[LoadProposal]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [Profit].[LoadProposal]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profit].[LoadProposalChecklist]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [Profit].[LoadProposalChecklist]
GO

IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[Profit].[LoadProposalPermission]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [Profit].[LoadProposalPermission]
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'DataMigration')
	DROP TABLE [dbo].[DataMigration]
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'foresightProgram')
ALTER TABLE [dbo].[foresightProgram] DROP CONSTRAINT [PK_ForesightProgram]
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'foresightProgram')
	DROP TABLE [dbo].[foresightProgram]
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'MissingIndex')
	DROP TABLE [dbo].[MissingIndex]
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'PricerReorgChangeRequestSpreadsheet')
	DROP TABLE [dbo].[PricerReorgChangeRequestSpreadsheet]
GO 

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'ProposalReorganization')
	DROP INDEX [IX_ProposalReorganization_C1] ON [dbo].[ProposalReorganization]
GO 

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'ProposalReorganization')
	ALTER TABLE [dbo].[ProposalReorganization] DROP CONSTRAINT [PK_ProposalReorganization]
GO 

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo'  AND  TABLE_NAME = 'ProposalReorganization')
	DROP TABLE [dbo].[ProposalReorganization]
GO 

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Profit'  AND  TABLE_NAME = 'Common')
	DROP TABLE [Profit].[Common]
GO 

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Profit'  AND  TABLE_NAME = 'ProposalLog')
	DROP TABLE [Profit].[ProposalLog]
GO 

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'Profit'  AND  TABLE_NAME = 'User')
	DROP TABLE [Profit].[User]
GO 

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'TheGrid'  AND  TABLE_NAME = 'Employee')
	DROP TABLE [TheGrid].[Employee]
GO

-- Possibly also remove the TempPAR_International and TempPPR_International tables as well.  

/*
	1/16/2017 [twilson3] - BOEJ-1604 - Cleanup DB Project

	## END ##
*/ 

/*
	## START ##
	
	1/23/2017 [twilson3] - BOEJ-1738 Additional Info for Approval Emails
*/

IF NOT EXISTS (
				SELECT * FROM sys.all_columns C
					INNER JOIN sys.tables T on C.object_id = T.object_id
					INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE
					T.name = 'Proposal' AND
					C.name = 'ApprovalEmailText' AND
					S.name = 'dbo'
				)
BEGIN
	ALTER TABLE [dbo].[Proposal]
		ADD [ApprovalEmailText] varchar(1000) NULL
	
END
GO

/*
	1/23/2017 [twilson3] - BOEJ-1738 Additional Info for Approval Emails

	## END ##
*/

/*
	## START ##

	2/2/2017 [pattoncr] -- BOEJ-1744 Update "Where is the proposal team located?" options on proposal details page
*/

-- Update old IS&GS Proposal Locations to mark them inactive.
update [dbo].[ProposalLocationLU] set IsActive = 0 where IsActive = 1 and ProposalLocation in (
	'Rockville, MD',
	'Chantilly, VA',
	'Colorado Spring, CO',
	'Hanover, MD',
	'Herndon, VA',
	'Littleton, CO',
	'Rockville, MD',
	'Virtual'
);

-- Insert new Proposal Location values.
IF NOT EXISTS (SELECT * FROM [dbo].[ProposalLocationLU] WHERE [ProposalLocation] = 'Cape Canaveral, FL')
BEGIN
	INSERT INTO [dbo].[ProposalLocationLU] ([ProposalLocation],[IsActive]) VALUES ('Cape Canaveral, FL', '1')
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[ProposalLocationLU] WHERE [ProposalLocation] = 'Denver, CO')
BEGIN
	INSERT INTO [dbo].[ProposalLocationLU] ([ProposalLocation],[IsActive]) VALUES ('Denver, CO', '1')
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[ProposalLocationLU] WHERE [ProposalLocation] = 'Huntsville, AL')
BEGIN
	INSERT INTO [dbo].[ProposalLocationLU] ([ProposalLocation],[IsActive]) VALUES ('Huntsville, AL', '1')
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[ProposalLocationLU] WHERE [ProposalLocation] = 'Michoud, LA')
BEGIN
	INSERT INTO [dbo].[ProposalLocationLU] ([ProposalLocation],[IsActive]) VALUES ('Michoud, LA', '1')
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[ProposalLocationLU] WHERE [ProposalLocation] = 'Sunnyvale, CA')
BEGIN
	INSERT INTO [dbo].[ProposalLocationLU] ([ProposalLocation],[IsActive]) VALUES ('Sunnyvale, CA', '1')
END
GO

/*
	2/2/2017 [pattoncr] -- BOEJ-1744 Update "Where is the proposal team located?" options on proposal details page
	
	## END ##
*/
/*
	## START ##

	2/2/2017 [twilson3] - BOEJ-1794 Remove Delinquent Checklist Report
*/
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateDelinquentChecklistReport]') AND type in (N'P', N'PC'))
BEGIN
	DROP VIEW [dbo].[vwDelinquentChecklistReport];
	DROP PROCEDURE [dbo].[CreateDelinquentChecklistReport];
END
GO

/*
	2/2/2017 [twilson3] - BOEJ-1794 Remove Delinquent Checklist Report

	## END ##
*/

/*
	## START ##

	2/6/2017 [Tom] - Adding 2 new fields to PTM
*/
IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE S.name = 'dbo' AND
					T.name = 'ProposalChecklist' AND
					C.name = 'AbsoluteValue'					
				)
BEGIN
	ALTER TABLE dbo.ProposalChecklist ADD AbsoluteValue bigint NULL
	ALTER TABLE dbo.ProposalChecklist SET (LOCK_ESCALATION = TABLE)
END
GO

IF NOT EXISTS (SELECT * FROM sys.all_columns C INNER JOIN sys.tables T on C.object_id = T.object_id INNER JOIN sys.schemas S ON T.schema_id =  S.schema_id
				WHERE S.name = 'dbo' AND
					T.name = 'Proposal' AND
					C.name = 'RevisedSubmittalDate'				
				)
BEGIN
	ALTER TABLE dbo.Proposal ADD RevisedSubmittalDate date NULL
	ALTER TABLE dbo.Proposal SET (LOCK_ESCALATION = TABLE)
END
GO

/*
	2/6/2017 [Tom] - Adding 2 new fields to PTM

	## END ##
*/


/*
	## START ##
	
	01/31/2017 [brunworg] - BOEJ-1768 and BOEJ-1773 - PPR and PAR checklist changes 
*/
IF NOT EXISTS (SELECT 1 FROM [dbo].[ProposalPricingReview] WHERE [ChecklistVersion] = 9)
BEGIN

	BEGIN TRANSACTION
	BEGIN TRY
		-- Deactivate International Proposal Checklist Type
		UPDATE [dbo].[ProposalChecklistTypeLU]
		set [IsActive] = 0
		WHERE [ProposalChecklistTypeID] = 2 

		-- Deactivate previous PPR versions
		UPDATE [dbo].[ProposalPricingReview] 
		SET [IsCurrent] = 0

		-- Create new PPR version
		SET IDENTITY_INSERT [dbo].[ProposalPricingReview] ON
		INSERT [dbo].[ProposalPricingReview] ([ProposalPricingReviewID], [ChecklistVersion], [IsCurrent], [ProposalChecklistTypeID]) VALUES (10, 9, 1, 1)
		SET IDENTITY_INSERT [dbo].[ProposalPricingReview] OFF

		-- Deactivate previous PAR version
		UPDATE [dbo].[ProposalAdequacyReview]
		SET [IsCurrent] = 0

		-- Create new PAR version
		SET IDENTITY_INSERT [dbo].[ProposalAdequacyReview] ON
		INSERT [dbo].[ProposalAdequacyReview] ([ProposalAdequacyReviewID], [ChecklistVersion], [IsCurrent], [ProposalChecklistTypeID]) VALUES (10, 9, 1, 1)
		SET IDENTITY_INSERT [dbo].[ProposalAdequacyReview] OFF

		-- Insert PPR Checklist rows
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>&nbsp;<span style="font-style:italic; font-weight: bold;">To be completed by the Lead Estimator when Certified Cost or Pricing Data is Required.</span></p><br/>', 1, 1, 1, 10)
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>&nbsp;<span style="font-style:italic; font-weight: bold; color:red">Note: If you answer "No" to any of these questions, provide an explanation in the Comments section below.</span></p>', 1, 2, 1, 10)
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<b>Proposal Pricing Review Items</b> ', 2, 3, 1, 10)
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>1. Is the application of Fee evident? </p>', 4, 4, 1, 10)
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>2. Are contract closeout costs included in the estimate, or have such costs been deferred by contractual provision? </p>', 4, 5, 1, 10)
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>3. Does the proposal contain the appropriate proprietary data legends, including legends on any electronic media and on any PC-based cost model (e.g., Excel) spreadsheets? </p>', 4, 6, 1, 10)
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>4. Have all IWTA Facilities Capital Cost of Money (FCCM) been separately identified and excluded from the base for fee calculations? </p>', 4, 7, 1, 10)
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>5. Is the application shown of any rate not provided on the cost summaries, as well as escalation (including hours/dollars base, factor, and resulting hours/dollars)? </p>', 4, 8, 1, 10)
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>6. Are the derivation/source of skill mix, and the resulting composite activity type labor rate provided? </p>', 4, 9, 1, 10)
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>7. Are the applicable mandatory disclosure items included in the proposal? </p>', 4, 10, 1, 10)
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>8. Did you provide the PREMIUM overtime dollars, to Contracts, required to complete the FAR contract clause 22.103-5(b)? </p>', 4, 11, 1, 10)
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p>9. Did you include the solicitation # and the CAGE code on your cover sheet? </p>', 4, 12, 1, 10)
		INSERT [dbo].[PPRChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalPricingReviewID]) VALUES (N'<p><span style="font-style:italic; font-weight: bold;">Comments required for all “No” responses. All information must be unclassified and non-export controlled.</span></p>', 5, 13, 1, 10)


		-- Insert PAR Checklist rows
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<span style="font-style:italic; font-weight: bold;">To be completed by the Lead Estimator for all Proposals that require Certified Cost or Pricing Data.</span>', 1, 1, 1, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<span style="color: #002060; font-style:italic; font-weight: bold; font-size: 10px;">&nbsp;&nbsp;- This includes all IWTA proposals, regardless of value, if Prime requires Certified Cost or Pricing Data</span>', 1, 2, 1, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<br/><span style="color: red; font-style:italic; font-weight:bold;">Note: Answer "Yes" if items are included in proposal. If any of the items are not included, select "No" and provide an explanation in the Comments section.</span><br/>', 1, 3, 1, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<br/>', 1, 4, 1, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<b>DFARS Proposal Adequacy Checklist Items</b> ', 2, 5, 1, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<b>General Instructions</b>', 2, 5, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>General Instructions</b></p>', 2, 6, 1, 10, NULL, NULL, 'GENERAL INSTRUCTIONS')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 6, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p> 1. Is there a properly completed first page of the proposal per FAR 15.408 Table 15-2 I.A or as specified in the solicitation? (FAR 15.408, Table 15-2, Section I Paragraph A) </p>', 4, 7, 1, 10, '1', 'FAR 15.408, Table 15-2, Section I Paragraph A', 'Is there a properly completed first page of the proposal per FAR 15.408 Table 15-2 I.A or as specified in the solicitation?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><span style="color: red;">Use the SSC Cover Sheet for CCOPD available on the Estimating SharePoint. This cover sheet shall be page one of your proposal.</span></p><br /><p><span style="color: red;">A separate breakout by CLIN may be included as an attachment. Ensure all fields are completed.</span></p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_01.docx'' target=''_blank'' target=''_blank''>Additional Instructions</a></p>', 4, 7, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>2. Does the proposal identify the need for Government-furnished material/tooling/test equipment? Include the accountable contract number and contracting officer contact information if known. (FAR 15.408, Table 15-2, Section I Paragraph A(7))</p>', 4, 8, 1, 10, '2', 'FAR 15.408, Table 15-2, Section I Paragraph A(7)', 'Does the proposal identify the need for Government-furnished material/tooling/test equipment? Include the accountable contract number and contracting officer contact information if known. ')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><span style="color: red;">This criteria is applicable for existing and new Government-furnished material/tooling/test equipment.</span></p><br /><p><span style="color: red;">Include the accountable contract number and contracting officer contact information if known.</span></p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_02.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 8, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>3. Does the proposal identify and explain notifications of noncompliance with Cost Accounting Standards Board or Cost Accounting Standards (CAS); any proposal inconsistencies with your disclosed practices or applicable CAS; and inconsistencies with your established estimating and accounting principles and procedures? (FAR 15.408, Table 15-2, Section I Paragraph A(8)) </p>', 4, 9, 1, 10, '3', 'FAR 15.408, Table 15-2, Section I Paragraph A(8)', 'Does the proposal identify and explain notifications of noncompliance with Cost Accounting Standards Board or Cost Accounting Standards (CAS); any proposal inconsistencies with your disclosed practices or applicable CAS; and inconsistencies with your established estimating and accounting principles and procedures?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Provide SSC Disclosures for any CAS violations or inconsistencies.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_03.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 9, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>4. Does the proposal disclose any other known activity that could materially impact the costs? This may include, but is not limited to, such factors as— <br />  (1) Vendor quotations;<br />  (2) Nonrecurring costs;<br />  (3) Information on changes in production methods and in production or purchasing volume;<br />  (4) Data supporting projections of business prospects and objectives and related operations costs;<br />  (5) Unit-cost trends such as those associated with labor efficiency;<br />  (6) Make-or-buy decisions;<br />  (7) Estimated resources to attain business goals; and<br />  (8) Information on management decisions that could have a significant bearing on costs.<br />  (FAR 15.408, Table 15-2, Section I, Paragraph C(1); FAR 2.101, “Cost or pricing data”)  </p>', 4, 10, 1, 10, '4', 'FAR 15.408, Table 15-2, Section I, Paragraph C(1); FAR 2.101, Cost or pricing data', 'Does the proposal disclose any other known activity that could materially impact the costs? This may include, but is not limited to, such factors as— 
 (1) Vendor quotations;
 (2) Nonrecurring costs;
 (3) Information on changes in production methods and in production or purchasing volume;
 (4) Data supporting projections of business prospects and objectives and related operations costs;
 (5) Unit-cost trends such as those associated with labor efficiency;
 (6) Make-or-buy decisions;
 (7) Estimated resources to attain business goals; and
 (8) Information on management decisions that could have a significant bearing on costs.')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Provide all applicable SSC Disclosures.</p><p>All proposal must be priced based on current information. If specific information is known that is not included in pricing this information must be disclosed and referenced to in the comment section.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_04.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 10, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>5. Is an Index of all certified cost or pricing data and information accompanying or identified in the proposal provided and appropriately referenced? (FAR 15.408, Table 15-2, Section I Paragraph B)</p>', 4, 11, 1, 10, '5', 'FAR 15.408, Table 15-2, Section I Paragraph B', 'Is an Index of all certified cost or pricing data and information accompanying or identified in the proposal provided and appropriately referenced?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Table of Contents with Section references is required.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_05.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 11, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>6. Are there any exceptions to submission of certified cost or pricing data pursuant to FAR 15.403-1(b)? If so, is supporting documentation included in the proposal? (Note questions 18-20.) (FAR 15.403-1(b))</p>', 4, 12, 1, 10, '6', 'FAR 15.403-1(b)', 'Are there any exceptions to submission of certified cost or pricing data pursuant to FAR 15.403-1(b)? If so, is supporting documentation included in the proposal? (Note questions 18-20.)')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Are there any exceptions to submission of certified cost or pricing data within the proposal (i.e. Subs/Material) in accordance with FAR 15.403-1(b)?<br />  (1) When prices agreed upon are based on adequate price competition <br />  (2) When prices agreed upon are based on prices set by law or regulation <br />   (3) When a commercial item is being acquired <br />  (4) When a waiver has been granted <span style="color: red;"><u>(by the head of the government contracting agency)</u></span>; or<br />  (5) When modifying a contract or subcontract for commercial items<br /><br />If Yes, included supporting documentation in the cost volume (Note questions 18-20)</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_06.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 12, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>7. Does the proposal disclose the judgmental factors applied and the mathematical or other methods used in the estimate, including those used in projecting from known data? (FAR 15.408, Table 15-2, Section I Paragraph C(2)(i))</p>', 4, 13, 1, 10, '7', 'FAR 15.408, Table 15-2, Section I Paragraph C(2)(i)', 'Does the proposal disclose the judgmental factors applied and the mathematical or other methods used in the estimate, including those used in projecting from known data?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Ensure BOEs are documented and any judgment or factors are fully explained. References all sections with BOEs.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_07.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 13, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>8. Does the proposal disclose the nature and amount of any contingencies included in the proposed price? (FAR 15.408, Table 15-2, Section I Paragraph C(2)(ii))</p>', 4, 14, 1, 10, '8', 'FAR 15.408, Table 15-2, Section I Paragraph C(2)(ii)', 'Does the proposal disclose the nature and amount of any contingencies included in the proposed price? ')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>The LOB Estimating manager and Central Estimating should be fully informed of the nature and amount of any contingencies proposed.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_08.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 14, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>9. Does the proposal explain the basis of all cost estimating relationships (labor hours or material) proposed on other than a discrete basis? (FAR 15.408 Table 15-2, Section II, Paragraph A or B)</p>', 4, 15, 1, 10, '9', 'FAR 15.408 Table 15-2, Section II, Paragraph A or B', 'Does the proposal explain the basis of all cost estimating relationships (labor hours or material) proposed on other than a discrete basis?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Insert section # of any Cost Estimating Relationship (CER) and/or Historical Experience Factor (HEF).</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_09.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 15, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>10. Is there a summary of total cost by element of cost and are the elements of cost cross-referenced to the supporting cost or pricing data? (Breakdowns for each cost element must be consistent with your cost accounting system, including breakdown by year.) (FAR 15.408, Table 15-2, Section I Paragraphs D and E)</p>', 4, 16, 1, 10, '10', 'FAR 15.408, Table 15-2, Section I Paragraphs D and E', 'Is there a summary of total cost by element of cost and are the elements of cost cross-referenced to the supporting cost or pricing data? (Breakdowns for each cost element must be consistent with your cost accounting system, including breakdown by year.)')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Total Cost Summary by Element of Cost, indexed to supporting documentation in the Cost Volume.</p><p>Total Cost Element Summary by CY (and GFY if required).</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_10.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 16, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>11. If more than one Contract Line Item Number (CLIN) or sub Contract Line Item Number (sub-CLIN) is proposed as required by the RFP, are there summary total amounts covering all line items for each element of cost and is it cross-referenced to the supporting cost or pricing data? (FAR 15.408, Table 15-2, Section I Paragraphs D and E)</p>', 4, 17, 1, 10, '11', 'FAR 15.408, Table 15-2, Section I Paragraphs D and E', 'If more than one Contract Line Item Number (CLIN) or sub Contract Line Item Number (sub-CLIN) is proposed as required by the RFP, are there summary total amounts covering all line items for each element of cost and is it cross-referenced to the supporting cost or pricing data?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Total Cost Summary by Element of Cost for each CLIN/sub-CLIN (if applicable), indexed to supporting documentation in the Cost Volume.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_11.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 17, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>12. Does the proposal identify any incurred costs for work performed before the submission of the proposal? (FAR 15.408, Table 15-2, Section I Paragraph F)</p>', 4, 18, 1, 10, '12', 'FAR 15.408, Table 15-2, Section I Paragraph F', 'Does the proposal identify any incurred costs for work performed before the submission of the proposal?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>There must be a separate cost report (priced with current pricing rates, for the year being priced) that identifies actual costs already performed. </p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_12.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 18, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>13. Is there a Government forward pricing rate agreement (FPRA)? If so, the offeror shall identify the official submittal of such rate and factor data. If not, does the proposal include all rates and factors by year that are utilized in the development of the proposal and the basis for those rates and factors? (FAR 15.408, Table 15-2, Section I Paragraph G)</p>', 4, 19, 1, 10, '13', 'FAR 15.408, Table 15-2, Section I Paragraph G', 'Is there a Government forward pricing rate agreement (FPRA)? If so, the offeror shall identify the official submittal of such rate and factor data. If not, does the proposal include all rates and factors by year that are utilized in the development of the proposal and the basis for those rates and factors?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Refer to Rates and Factor Section where the FPRA’s are referenced.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_13.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 19, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Cost Elements</b></p>', 2, 20, 1, 10, NULL, NULL, 'COST ELEMENTS')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 20, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Materials and Services</b></p>', 2, 21, 1, 10, NULL, NULL, 'Materials and Services')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 21, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>14. Does the proposal include a consolidated summary of individual material and services, frequently referred to as a Consolidated Bill of Material (CBOM), to include the basis for pricing? The offeror’s consolidated summary shall include raw materials, parts, components, assemblies, subcontracts and services to be produced or performed by others, <u>identifying as a minimum the item, source, quantity, and price.</u> (FAR 15.408, Table 15-2, Section II Paragraph A)</p>', 4, 22, 1, 10, '14', 'FAR 15.408, Table 15-2, Section II Paragraph A', 'Does the proposal include a consolidated summary of individual material and services, frequently referred to as a Consolidated Bill of Material (CBOM), to include the basis for pricing? The offeror’s consolidated summary shall include raw materials, parts, components, assemblies, subcontracts and services to be produced or performed by others, identifying as a minimum the item, source, quantity, and price.')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>CBOM is required for all proposals that include Material and Subcontracts.  In either descending dollar or part number order.</p><br /><p>Separate CBOM may be required to identify CLIN pricing.</p><br /><p>IWTAs are not included on CBOM unless required by RFP.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_14.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 22, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Subcontracts</b></p>', 2, 23, 1, 10, NULL, NULL, 'SUBCONTRACTS (Purchased materials or services)')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 23, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>15. Has the offeror identified in the proposal those subcontractor proposals, for which the contracting officer has initiated or may need to request field pricing analysis? (DFARS 215.404-3)</p>', 4, 24, 1, 10, '15', 'DFARS 215.404-3', 'Has the offeror identified in the proposal those subcontractor proposals, for which the contracting officer has initiated or may need to request field pricing analysis?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Reference Subcontract Summary Table, if the cost volume includes subcontracts. </p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_15.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 24, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>16. Per the thresholds of FAR 15.404-3(c), Subcontract Pricing Considerations, does the proposal include a copy of the applicable subcontractor’s certified cost or pricing data? (FAR 15.404-3(c); FAR 52.244-2)</p>', 4, 25, 1, 10, '16', 'FAR 15.404-3(c); FAR 52.244-2', 'Per the thresholds of FAR 15.404-3(c), Subcontract Pricing Considerations, does the proposal include a copy of the applicable subcontractor’s certified cost or pricing data?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Subcontractor Proposals (If S/C proposal ? $13.5M or if S/C proposal ? $750K and 10% of the Prime Proposal price) Must be included with proposal or include statement how the subcontracts are submitted. </p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_16.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 25, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>17. Is there a price/cost analysis establishing the reasonableness of each of the proposed subcontracts included with the proposal? If the offeror’s price/cost analyses are not provided with the proposal, does the proposal include a matrix identifying dates for receipt of subcontractor proposal, completion of fact finding for purposes of price/cost analysis, and submission of the price/cost analysis? (FAR 15.408, Table 15-2, Note 1; Section II Paragraph A)</p>', 4, 26, 1, 10, '17', 'FAR 15.408, Table 15-2, Note 1; Section II Paragraph A', 'Is there a price/cost analysis establishing the reasonableness of each of the proposed subcontracts included with the proposal? If the offeror’s price/cost analyses are not provided with the proposal, does the proposal include a matrix identifying dates for receipt of subcontractor proposal, completion of fact finding for purposes of price/cost analysis, and submission of the price/cost analysis?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Reference to Subcontract summary with references to Price analysis, Cost Analysis, PBOEs.  PBOEs are required for Subcontracts and for any material items over the CCOPD threshold. </p><br /><p>Cost Analysis are required for all subcontracts over CCOPD unless you get written concurrence, from the customer, to provide at a later date.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_17.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 26, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Exceptions to Certified Cost or Pricing Data</b></p>', 2, 27, 1, 10, NULL, NULL, 'EXCEPTIONS TO CERTIFIED COST OR PRICING DATA')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 27, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>18. Has the offeror submitted an exception to the submission of certified cost or pricing data for commercial items proposed either at the prime or subcontractor level, in accordance with provision 52.215-20?<br />  a.  Has the offeror specifically identified the type of commercial item claim (FAR 2.101 commercial item definition, paragraphs (1) through (8)), and the basis on which the item meets the definition?<br />  b.  For modified commercial items (FAR 2.101 commercial item definition paragraph (3)); did the offeror classify the modification(s) as either—<br />  &nbsp;&nbsp;i.  A modification of a type customarily available in the commercial marketplace (paragraph (3)(i)); or<br />  &nbsp;&nbsp;ii.  A minor modification (paragraph (3)(ii)) of a type not customarily available in the commercial marketplace made to meet Federal Government requirements not exceeding the thresholds in FAR 15.403-1(c)(3)(iii)(B)?<br />  c.  For proposed commercial items "of a type", or "evolved" or modified (FAR 2.101 commercial item definition paragraphs (1) through (3)), did the contractor provide a technical description of the differences between the proposed item and the comparison item(s)?<br />  (FAR 52.215-20; FAR 2.101, "commercial item")</p>', 4, 28, 1, 10, '18', 'FAR 52.215-20; FAR 2.101, commercial item', 'Has the offeror submitted an exception to the submission of certified cost or pricing data for commercial items proposed either at the prime or subcontractor level, in accordance with provision 52.215-20? 
a.  Has the offeror specifically identified the type of commercial item claim (FAR 2.101 commercial item definition, paragraphs (1) through (8)), and the basis on which the item meets the definition?
b.  For modified commercial items (FAR 2.101 commercial item definition paragraph (3));  did the offeror classify the modification(s) as either—
    i.  A modification of a type customarily available in the commercial marketplace (paragraph (3)(i)); or 
    ii.  A minor modification (paragraph (3)(ii)) of a type not customarily available in the commercial marketplace made to meet Federal Government requirements not exceeding the thresholds in FAR 15.403-1(c)(3)(iii)(B)?
c.  For proposed commercial items “of a type”, or “evolved” or modified (FAR 2.101 commercial item definition paragraphs (1) through (3)), did the contractor provide a technical description of the differences between the proposed item and the comparison item(s)?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>If a supplier submits a proposal that includes a claim for commercial item exception for an item that is over the CCOPD threshold  include the following: LMAP Forms F 335, F 340; F 345 (as applicable).</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_18.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 28, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>19.<span style="color: red;">[Reserved]</span></p>', 4, 29, 1, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_19.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 29, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>20. Does the proposal support the degree of competition and the basis for establishing the source and reasonableness of price for each subcontract or purchase order priced on a competitive basis exceeding the threshold for certified cost or pricing data? (FAR 15.408, Table 15-2, Section II Paragraph A(1))</p>', 4, 30, 1, 10, '20', 'FAR 15.408, Table 15-2, Section II Paragraph A(1)', 'Does the proposal support the degree of competition and the basis for establishing the source and reasonableness of price for each subcontract or purchase order priced on a competitive basis exceeding the threshold for certified cost or pricing data?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>PBOE, IBOE or CBOM must clearly state the degree of competition</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_20.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 30, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Interorganizational Transfers</b></p>', 2, 31, 1, 10, NULL, NULL, 'INTERORGANIZATIONAL TRANSFERS')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 31, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>21. For inter-organizational transfers proposed at cost, does the proposal include a complete cost proposal in compliance with Table 15-2? (FAR 15.408, Table 15-2, Section II Paragraph A.(2))</p>', 4, 32, 1, 10, '21', 'FAR 15.408, Table 15-2, Section II Paragraph A.(2)', 'For inter-organizational transfers proposed at cost, does the proposal include a complete cost proposal in compliance with Table 15-2?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Regardless of the IWTA value you must provide a FAR 15.408, Table 15-2, compliant IWTA Proposal</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_21.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 32, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>22. For inter-organizational transfers proposed at price in accordance with FAR 31.205-26(e), does the proposal provide an analysis by the prime that supports the exception from certified cost or pricing data in accordance with FAR 15.403-1? (FAR 15.408, Table 15-2, Section II Paragraph A(1))</p>', 4, 33, 1, 10, '22', 'FAR 15.408, Table 15-2, Section II Paragraph A(1)', 'For inter-organizational transfers proposed at price in accordance with FAR 31.205-26(e), does the proposal provide an analysis by the prime that supports the exception from certified cost or pricing data in accordance with FAR 15.403-1?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>IBOE must provide the analysis to support an IWTA "P" </p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_22.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 33, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Direct Labor</b></p>', 2, 34, 1, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 34, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>23. Does the proposal include a time phased (i.e.; monthly, quarterly) breakdown of labor hours, rates and costs by category or skill level? If labor is the allocation base for indirect costs, the labor cost must be summarized in order that the applicable overhead rate can be applied. (FAR 15.408, Table 15-2, Section II Paragraph B)</p>', 4, 35, 1, 10, '23', 'FAR 15.408, Table 15-2, Section II Paragraph B', 'Does the proposal include a time phased (i.e.; monthly, quarterly) breakdown of labor hours, rates and costs by category or skill level? If labor is the allocation base for indirect costs, the labor cost must be summarized in order that the applicable overhead rate can be applied.')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Normal practice is to summarize cost at an annual level that ties back to the BOE.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_23.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 35, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>24. For labor Basis of Estimates (BOEs), does the proposal include labor categories, labor hours, and task descriptions, (e.g.; Statement of Work reference, applicable CLIN, Work Breakdown Structure, rationale for estimate, applicable history, and time-phasing)? (FAR 15.408, Table 15-2, Section II Paragraph B)</p>', 4, 36, 1, 10, '24', 'FAR 15.408, Table 15-2, Section II Paragraph B', 'For labor Basis of Estimates (BOEs), does the proposal include labor categories, labor hours, and task descriptions, (e.g.; Statement of Work reference, applicable CLIN, Work Breakdown Structure, rationale for estimate, applicable history, and time-phasing)?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_24.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 36, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>25. If covered by the Service Contract Labor Standards statute (41 U.S.C. chapter 67), are the rates in the proposal in compliance with the minimum rates specified in the statute? (FAR subpart 22.10)</p>', 4, 37, 1, 10, '25', 'FAR subpart 22.10', 'If covered by the Service Contract Labor Standards statute (41 U.S.C. chapter 67), are the rates in the proposal in compliance with the minimum rates specified in the statute?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_25.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 37, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Indirect Costs</b></p>', 2, 38, 1, 10, NULL, NULL, 'INDIRECT COSTS')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 38, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>26. Does the proposal indicate the basis of estimate for proposed indirect costs and how they are applied? (Support for the indirect rates could consist of cost breakdowns, trends, and budgetary data.) (FAR 15.408, Table 15-2, Section II Paragraph C)</p>', 4, 39, 1, 10, '26', 'FAR 15.408, Table 15-2, Section II Paragraph C', 'Does the proposal indicate the basis of estimate for proposed indirect costs and how they are applied? (Support for the indirect rates could consist of cost breakdowns, trends, and budgetary data.)')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Current status of all rates used in the proposal must be provided. Pricing must show application of rates.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_26.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 39, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Other Costs</b></p>', 2, 40, 1, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 40, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>27. Does the proposal include other direct costs and the basis for pricing? If travel is included does the proposal include number of trips, number of people, number of days per trip, locations, and rates (e.g. airfare, per diem, hotel, car rental, etc)? (FAR 15.408, Table 15-2, Section II Paragraph D)</p>', 4, 41, 1, 10, '27', 'FAR 15.408, Table 15-2, Section II Paragraph D', 'Does the proposal include other direct costs and the basis for pricing? If travel is included does the proposal include number of trips, number of people, number of days per trip, locations, and rates (e.g. airfare, per diem, hotel, car rental, etc)?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>All ODC BOE must be included in proposal.  If Travel is estimated with the use of a HEF the backup analysis must be provided.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_27.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 41, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>28. If royalties exceed $1,500 does the proposal provide the information/data identified by Table 15-2? (FAR 15.408, Table 15-2, Section II Paragraph E)</p>', 4, 42, 1, 10, '28', 'FAR 15.408, Table 15-2, Section II Paragraph E', ' If royalties exceed $1,500 does the proposal provide the information/data identified by Table 15-2? ')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_28.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 42, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>29. When facilities capital cost of money is proposed, does the proposal include submission of Form CASB-CMF or reference to an FPRA/FPRP and show the calculation of the proposed amount? (FAR 15.408, Table 15-2, Section II Paragraph F)</p>', 4, 43, 1, 10, '29', 'FAR 15.408, Table 15-2, Section II Paragraph F', ' When facilities capital cost of money is proposed, does the proposal include submission of Form CASB-CMF or reference to an FPRA/FPRP and show the calculation of the proposed amount?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Provide 1861 forms which show the calculations of the proposed FCCOM/CAS 414 cost. Ensure reference letter are included for the latest FCCOM/CAS 414 rates.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_29.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 43, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Formats For Submission of Line Item Summaries</b></p>', 2, 44, 1, 10, NULL, NULL, 'FORMATS FOR SUBMISSION OF LINE ITEM SUMMARIES')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 44, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>30. Are all cost element breakdowns provided using the applicable format prescribed in FAR 15.408, Table 15-2 III? (or alternative format if specified in the request for proposal) (FAR 15.408, Table 15-2, Section III)</p>', 4, 45, 1, 10, '30', 'FAR 15.408, Table 15-2, Section III', 'Are all cost element breakdowns provided using the applicable format prescribed in FAR 15.408, Table 15-2 III? (or alternative format if specified in the request for proposal)')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Ensure you have the correct FAR 15.408, 15-2 format</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_30.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 45, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>31. If the proposal is for a modification or change order, have cost of work deleted (credits) and cost of work added (debits) been provided in the format described in FAR 15.408, Table 15-2.III.B? (FAR 15.408, Table 15-2, Section III Paragraph B)</p>', 4, 46, 1, 10, '31', 'FAR 15.408, Table 15-2, Section III Paragraph B', 'If the proposal is for a modification or change order, have cost of work deleted (credits) and cost of work added (debits) been provided in the format described in FAR 15.408, Table 15-2.III.B?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_31.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 46, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>32. For price revisions/redeterminations, does the proposal follow the format in FAR 15.408, Table 15-2.III.C?</p>', 4, 47, 1, 10, '32', 'FAR 15.408, Table 15-2, Section III Paragraph C', 'For price revisions/redeterminations, does the proposal follow the format in FAR 15.408, Table 15-2.III.C?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_32.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 47, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><b>Other</b></p>', 2, 48, 1, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 2, 48, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>33. If an incentive contract type, does the proposal include offeror proposed target cost, target profit or fee, share ratio, and, when applicable, minimum/maximum fee, ceiling price? (FAR 16.4)</p>', 4, 49, 1, 10, '33', 'FAR 16.4', 'If an incentive contract type, does the proposal include offeror proposed target cost, target profit or fee, share ratio, and, when applicable, minimum/maximum fee, ceiling price?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>Obtain from contracts if applicable</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_33.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 49, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>34. If Economic Price Adjustments are being proposed, does the proposal show the rationale and application for the economic price adjustment? (FAR 16.203-4 and FAR 15.408 Table 15-2, Section II, Paragraphs A, B, C, and D)</p>', 4, 50, 1, 10, '34', 'FAR 16.203-4 and FAR 15.408 Table 15-2, Section II, Paragraphs A, B, C, and D', 'If Economic Price Adjustments are being proposed, does the proposal show the rationale and application for the economic price adjustment?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_34.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 50, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>35. If the offeror is proposing Performance-Based Payments did the offeror comply with FAR 52.232-28? (FAR 52.232-28)</p>', 4, 51, 1, 10, '35', 'FAR 52.232-28', 'If the offeror is proposing Performance-Based Payments did the offeror comply with FAR 52.232-28?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_35.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 51, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>36. Excessive Pass-through Charges– Identification of Subcontract Effort: If the offeror intends to subcontract more than 70% of the total cost of work to be performed, does the proposal identify: (i) the amount of the offeror’s indirect costs and profit applicable to the work to be performed by the proposed subcontractor(s); and (ii) a description of the added value provided by the offeror as related to the work to be performed by the proposed subcontractor(s)? (FAR 15.408(n); FAR 52.215-22; FAR 52.215-23)</p>', 4, 52, 1, 10, '36', 'FAR 15.408(n); FAR 52.215-22; FAR 52.215-23', 'Excessive Pass-through Charges– Identification of Subcontract Effort: If the offeror intends to subcontract more than 70% of the total cost of work to be performed, does the proposal identify: (i) the amount of the offeror’s indirect costs and profit applicable to the work to be performed by the proposed subcontractor(s); and (ii) a description of the added value provided by the offeror as related to the work to be performed by the proposed subcontractor(s)?')
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p>If Subcontract costs exceeds 70% of the total value of this proposal ensure you add a statement for "added value" that LMSSC is providing.</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_36.docx'' target=''_blank''>Additional Instructions</a></p>', 4, 52, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><span style="font-weight:bold; font-style:italic;">Lead Estimator Comments (<span style="color: #0070c0">comments required for all "No" responses</span>)  <span style="color: red">All information must be unclassified and non-export controlled.</span></span></p>', 5, 53, 1, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 5, 53, 2, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'<p><span style="font-weight:bold; font-style:italic;">Peer Reviewer Comments (<span style="color: #0070c0">comments required for all "No" responses</span>)  <span style="color: red">All information must be unclassified and non-export controlled.</span></span></p>', 6, 54, 1, 10, NULL, NULL, NULL)
		INSERT [dbo].[PARChecklistContent] ([ChecklistText], [TextTypeID], [SortOrder], [ColumnOrder], [ProposalAdequacyReviewID], [QuestionNumber], [Reference], [SubmissionItem]) VALUES (N'', 6, 54, 2, 10, NULL, NULL, NULL)

		COMMIT TRANSACTION
	
	END TRY

	BEGIN CATCH
		ROLLBACK TRANSACTION
	

		DECLARE @ErrorMessage varchar (500)
		SELECT @ErrorMessage = ERROR_MESSAGE()
		RAISERROR (
				@ErrorMessage, -- Message text.
				11, -- Severity,/*Severity Changed to 11*/
				1 -- State,
				)


		
		RETURN
	
	END CATCH
    																																		
END

/*
	01/31/2017 [brunworg] - BOEJ-1768 and BOEJ-1773 - PPR and PAR checklist changes 

	## END ##
*/

/*
	## START ##

	2/14/2017 [Dusan] - Make all text black for the PAR questions - BOEJ-1882
*/

UPDATE [dbo].[PARChecklistContent] 
	SET [ChecklistText] = '<p><span>Use the SSC Cover Sheet for CCOPD available on the Estimating SharePoint. This cover sheet shall be page one of your proposal.</span></p><br /><p><span>A separate breakout by CLIN may be included as an attachment. Ensure all fields are completed.</span></p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_01.docx'' target=''_blank'' target=''_blank''>Additional Instructions</a></p>'
	WHERE [ChecklistText] = '<p><span style="color: red;">Use the SSC Cover Sheet for CCOPD available on the Estimating SharePoint. This cover sheet shall be page one of your proposal.</span></p><br /><p><span style="color: red;">A separate breakout by CLIN may be included as an attachment. Ensure all fields are completed.</span></p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_01.docx'' target=''_blank'' target=''_blank''>Additional Instructions</a></p>';

UPDATE [dbo].[PARChecklistContent] 
	SET [ChecklistText] = '<p><span>This criteria is applicable for existing and new Government-furnished material/tooling/test equipment.</span></p><br /><p><span>Include the accountable contract number and contracting officer contact information if known.</span></p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_02.docx'' target=''_blank''>Additional Instructions</a></p>' 
	WHERE [ChecklistText] = '<p><span style="color: red;">This criteria is applicable for existing and new Government-furnished material/tooling/test equipment.</span></p><br /><p><span style="color: red;">Include the accountable contract number and contracting officer contact information if known.</span></p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_02.docx'' target=''_blank''>Additional Instructions</a></p>' ;

UPDATE [dbo].[PARChecklistContent] 
	SET [ChecklistText] = '<p>Are there any exceptions to submission of certified cost or pricing data within the proposal (i.e. Subs/Material) in accordance with FAR 15.403-1(b)?<br />  (1) When prices agreed upon are based on adequate price competition <br />  (2) When prices agreed upon are based on prices set by law or regulation <br />   (3) When a commercial item is being acquired <br />  (4) When a waiver has been granted <span><u>(by the head of the government contracting agency)</u></span>; or<br />  (5) When modifying a contract or subcontract for commercial items<br /><br />If Yes, included supporting documentation in the cost volume (Note questions 18-20)</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_06.docx'' target=''_blank''>Additional Instructions</a></p>' 
	WHERE [ChecklistText] = '<p>Are there any exceptions to submission of certified cost or pricing data within the proposal (i.e. Subs/Material) in accordance with FAR 15.403-1(b)?<br />  (1) When prices agreed upon are based on adequate price competition <br />  (2) When prices agreed upon are based on prices set by law or regulation <br />   (3) When a commercial item is being acquired <br />  (4) When a waiver has been granted <span style="color: red;"><u>(by the head of the government contracting agency)</u></span>; or<br />  (5) When modifying a contract or subcontract for commercial items<br /><br />If Yes, included supporting documentation in the cost volume (Note questions 18-20)</p><p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_06.docx'' target=''_blank''>Additional Instructions</a></p>';

UPDATE [dbo].[PARChecklistContent] 
	SET [ChecklistText] = '<p>19.<span>[Reserved]</span></p>' 
	WHERE [ChecklistText] = '<p>19.<span style="color: red;">[Reserved]</span></p>';

UPDATE [dbo].[PARChecklistContent] 
	SET [ChecklistText] = ''
	WHERE [ChecklistText] = '<p><a href=''https://space-migrate.p.external.lmco.com/sites/fbo/CCDME/Estimating/Checklist%20Reference%20Documents/Instruction_19.docx'' target=''_blank''>Additional Instructions</a></p>';

GO

/*
	2/14/2017 [Dusan] - Make all text black for the PAR questions - BOEJ-1882

	## END ##
*/

/*
	## START ##

	2/14/2017 [Joe] - Fixing an issue w/ question 19. BOEJ-1897
*/

UPDATE [genTrac].[dbo].[PARChecklistContent]
	SET [Reference] = 'Reserved', QuestionNumber='19'
	WHERE [checklisttext]='<p>19.<span>[Reserved]</span></p>';

/*
	2/14/2017 [Joe] - Fixing an issue w/ question 19. BOEJ-1897

	## END ##
*/

/*
	## START ##

	2/17/2017 [twilson3] - BOEJ-1903 removing unused stored proc
*/
IF  EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[rsFieldEstimator]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[rsFieldEstimator];
GO

/*
	2/17/2017 [twilson3] - BOEJ-1903 removing unused stored proc

	## END ##
*/

/*
	## START ##

	2/8/2017 [twilson3] - BOEJ-1811 New Program Areas for LOBs
*/
IF NOT EXISTS (SELECT 1 FROM dbo.[LineOfBusiness] WHERE [LineOfBusinessName] = 'ATC')
BEGIN
		DECLARE @PLID int 
		SET @PLID = (SELECT [ProductLineID] from [dbo].[ProductLine] where [ProductLineName]= N'ATC')
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'ATC', N'ATC', N'ATC', @PLID, -1, 1)

		SET @PLID = (SELECT [ProductLineID] from [dbo].[ProductLine] where [ProductLineName]= N'Comm Space')
		INSERT [dbo].[LineOfBusiness] ([LineOfBusinessName], [LineOfBusinessLongName], [LineOfBusinessURL], [ProductLineID], [ForesightLineOfBusinessID], [IsActive]) VALUES (N'Comm Space', N'Comm Space', N'Comm Space', @PLID, -1, 1)
END
/*
	2/8/2017 [twilson3] - BOEJ-1811 New Program Areas for LOBs

	## END ##
*/	
