EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.4';
GO

/*
		## START ##
		3/29/18 twilson3 - BOEJ-3035 - BOE Emails
*/

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Trigger' AND Object_ID = Object_ID(N'[dbo].[EmailLU]'))
BEGIN
	ALTER TABLE [dbo].[EmailLU]
	ADD [Trigger] VARCHAR(200) NOT NULL DEFAULT '';
END

GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'DefaultOn' AND Object_ID = Object_ID(N'[dbo].[EmailLU]'))
BEGIN
	ALTER TABLE [dbo].[EmailLU]
	ADD [DefaultOn] bit NOT NULL DEFAULT 0;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'ForcedOn' AND Object_ID = Object_ID(N'[dbo].[EmailLU]'))
BEGIN
	ALTER TABLE [dbo].[EmailLU]
	ADD [ForcedOn] bit NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Category' AND Object_ID = Object_ID(N'[dbo].[EmailLU]'))
BEGIN
	ALTER TABLE [dbo].[EmailLU]
	ADD [Category] VARCHAR(100) NOT NULL DEFAULT 'Other';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Recipient' AND Object_ID = Object_ID(N'[dbo].[EmailLU]'))
BEGIN
	ALTER TABLE [dbo].[EmailLU]
	ADD [Recipient] VARCHAR(150) NOT NULL DEFAULT '';
END
GO

DELETE FROM [dbo].[EmailLU] WHERE EmailID = 1;
UPDATE [dbo].[EmailLU] SET [Trigger] = 'Requesting Workspace Admin Permission', [Recipient] = 'Workspace Administrator(s)' WHERE EmailID = 20;
UPDATE [dbo].[EmailLU] SET [Trigger] = 'Workspace Restored from Backup', [Recipient] = 'Workspace Administrator(s), Author(s), Reviewer(s), Approver(s)' WHERE EmailID = 24;

IF NOT EXISTS (SELECT 1 FROM [dbo].[EmailLU] WHERE EmailID = 28)
BEGIN
	
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (28, 'genBOE: BOE Ready for Review', 'The author of the following BOE has requested your review and comments.<br/><br/>Workspace/Proposal: {0}<br/>WBS: {1} {2}<br/>BOE Title: {10}<br/>CLIN: {3} {4}<br/>BOE Description: {5}<br/>BOE Status: {6}<br/>Author: {7}<br/>Approver(s): {8}<br/><br/>{9}', 'BOE Submitted For Review', 0, NULL, 'Workflow', 'All Reviewer(s)');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (29, 'genBOE: BOE Author has responded to your comments', '{0} responded to your comments on the following BOE.<br/><br/>Workspace/Proposal: {1}<br/>WBS: {2} {3}<br/>BOE Title: {9}<br/>CLIN: {4} {5}<br/>BOE Description: {6}<br/>BOE Status: {7}<br/>{8}', 'Author Responded To Comment', 0, NULL, 'Workflow', 'Commenters on that BOE');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (30, 'genBOE: BOE has been approved by {0}', '{0} has approved the following BOE.<BR/><BR/>Workspace/Proposal: {1}<BR/>WBS: {2} {3}<BR/>BOE Title: {10}<BR/>CLIN: {4} {5}<BR/>BOE Description: {6}<BR/>BOE Status: {7}<BR/>Approvers’ Response:<BR/>{8}<BR/><BR/>{9}', 'BOE Approved', 0, NULL, 'Workflow', 'Author(s) on that BOE');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (31, 'genBOE: BOE has been rejected by {0}', '{0} has rejected the following BOE. No more approvals can be made and any previous approvals have been removed. The BOE has been placed in Draft and the Author can resume editing of the BOE.<BR/><BR/>Workspace/Proposal: {1}<BR/>WBS: {2} {3}<BR/>BOE Title: {9}<BR/>CLIN: {4} {5}<BR/>BOE Description: {6}<BR/>BOE Status: {7}<BR/><BR/>{8}', 'BOE Rejected', 0, NULL, 'Workflow', 'Author(s) on that BOE');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (32, 'genBOE: BOE Awaiting Approval', 'The Author has completed work on the following BOE.  You may approve the BOE or reject it to send it back to the Author for rework.<BR/><BR/>Workspace/Proposal: {0}<BR/>WBS: {1} {2}<BR/>BOE Title: {10}<BR/>CLIN: {3} {4}<BR/>BOE Description: {5}<BR/>BOE Status: {6}<BR/>Author: {7} <BR/> Approver(s): {8}<BR/><BR/>{9}', 'BOE Awaiting Approval', 0, NULL, 'Workflow', 'Approver(s) on that BOE');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (33, 'genBOE: All BOEs have been approved for the {0} Workspace', 'All BOEs in the Workspace have been approved.<BR/><BR/>Workspace/Proposal: {0}<BR/>Workspace Status: {1}<BR/><BR/>{2}', 'All BOEs Approved', 0, NULL, 'Workflow', 'Workspace Administrator(s)');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (34, 'genBOE: {0} submitted comments for BOE', '{0} submitted or updated comments for the following BOE: <BR/> Workspace/Proposal: {1} <BR/> WBS: {2} {3} <BR/> BOE Title: {9} <BR/> CLIN: {4} {5} <BR/> BOE Status: {6} <BR/> BOE Description: {7} <BR/><BR/>{8}', 'Reviewer Commented', 0, NULL, 'Workflow', 'Author(s) on that BOE');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (35, 'genBOE: BOE Open For Edits', 'The following BOE is now open for edits by the Author. The Author must submit the BOE to the Approver(s) once it is ready for approval. <BR/><BR/>Workspace/Proposal: {0}<BR/>WBS: {1} {2}<BR/>BOE Title: {9}<BR/>CLIN: {3} {4}<BR/>BOE: {5}<BR/>Author: {6} <BR/> Approver(s): {7}<BR/><BR/>{8}', 'BOE Opened For Edit (Workspace Status Changed from Initialization to Working & when BOE set back to Draft)', 0, NULL, 'Workflow', 'Author(s) on that BOE');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (36, 'genBOE: WBS Updated', 'Workspace: {0}<BR/>WBS: {1} {2}<BR/>BOE Title: {12}<BR/>CLIN: {3} {4}<BR/>BOE Description: {5}<BR/>BOE Status: {6}<BR/>Author: {7} <BR/>Approver(s): {8}<BR/><BR/> The following information has changed for the BOE by {9}. As the Author, you must verify the BOE is correct and resubmit it for approval.<BR/><BR/>{10}<BR/>{11}', 'WBS Update on save or via Import', 0, NULL, 'BOE Component Changes (WBS, CLIN, Resource, etc.)', 'BOE Author(s)');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (37, 'genBOE: CLIN Updated', 'Workspace: {0}<BR/>WBS: {1} {2}<BR/>BOE Title: {12}<BR/>CLIN: {3} {4}<BR/>BOE Description: {5}<BR/>BOE Status: {6}<BR/>Author: {7} <BR/>Approver(s): {8}<BR/><BR/> The following information has changed for the BOE by {9}. As the Author, you must verify the BOE is correct and resubmit it for approval.<BR/><BR/>{10}<BR/>{11}', 'CLIN Update on save or via Import', 0, NULL, 'BOE Component Changes (WBS, CLIN, Resource, etc.)', 'BOE Author(s)');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (38, 'genBOE: BOE Deleted', 'The following BOE has been deleted.<br/><br/>Workspace/Proposal: {0}<br/>WBS: {1} {2}<br/>BOE Title: {9}<br/>CLIN: {3} {4}<br/>BOE Description: {5}<br/>BOE Status: {6}<br/>Deleted By: {7}<br/><br/>{8}', 'BOE Deleted', 0, NULL, 'Other', 'Workspace Administrator(s), Author(s), Approver(s) on that BOE');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (39, 'genBOE: Resource used in the BOE has been modified by {0}', '{0} has modified resource {1} in the following BOEs. If additional changes are needed to the BOEs, please request the Workspace Administrator to move the BOEs back to Draft state. <BR/> <BR/> Workspace/Proposal: {2} <BR/><BR/> Resource Changes <BR/> {3} <BR/> {4} <BR/>', 'In Use Resource Edit on a BOE in Awaiting Approval/Approved State', 0, NULL, 'BOE Component Changes (WBS, CLIN, Resource, etc.)', 'Author(s) and Approver(s) of BOEs affected by a resource edit');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (40, 'genBOE: BOE reassigned to new Author(s)', 'The following BOE has been reassigned to new Author(s).  The newly assigned authors may now edit the BOE.<br/><br/>Workspace/Proposal: {0}<br/>WBS: {1} {2}<br/>BOE Title: {11}<br/>CLIN: {3} {4}<br/>BOE Description: {5}<br/>BOE Status: {6}<br/>Previous Authors: {7}<br/>Newly Assigned Authors: {8}<br/>Approver(s): {9}<br/><br/>{10}', 'BOE Author(s) Changed', 0, NULL, 'BOE Assignment Changes', 'New Author(s), Existing Author(s), Deleted Author(s)');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (41, 'genBOE: BOE reassigned to new Approver(s)', 'The following BOE has been reassigned to new Approver(s) and is awaiting approval as shown in the Approval Status below.<br/><br/>Workspace/Proposal: {0}<br/>WBS: {1} {2}<br/>BOE Title: {12}<br/>CLIN: {3} {4}<br/>BOE Description: {5}<br/>BOE Status: {6}<br/>Author: {7}<br/>Previous Approver(s): {8}<br/>Newly assigned Approver(s): {9}<br/>Approval Status:<br/>{10}<br/><br/>{11}', 'BOE Approver(s) Changed', 0, NULL, 'BOE Assignment Changes', 'New Approver(s), Existing Approver(s), Deleted Approver(s)');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (42, 'genBOE: BOE Start and End Date has been modified by {0}', '{0} has modified Start and /or End Dates for the following BOEs. BOEs that were Awaiting Approval or Approved are now in Draft.<BR/><BR/>Workspace/Proposal: {1}<BR/>{2}<BR/>{3}', 'Date Adjust', 0, NULL, 'Other', 'Author(s) on affected BOEs and Approver(s) on Awaiting Approval/Approved BOEs');
	INSERT INTO [dbo].[EmailLU] (EmailID, [Subject], Body, [Trigger], DefaultOn, ForcedOn, [Category], [Recipient])
		VALUES (43, 'genBOE: BOE''s CLIN or WBS has been modified by {0}', '{0} has {5} a BOE {1} with the following changes: {2}.<BR/>This may impact task variables listed below for BOE {3}<BR/>{4}', 'WBS or CLIN updated on a BOE', 0, NULL, 'Other', 'Author(s) with a Task that uses a Sum of BOEs task (not WS) variable that is summarizing by CLIN or WBS');

END
GO

-- These changes are to be executed in SSC only. The way we can tell the environments apart is that SSC has LOBs in the range of 1000's. RMS is 2000+ and ISGS is 0-999
IF EXISTS (SELECT 1 FROM [dbo].[LineOfBusiness] WHERE LineOfBusinessID > 1000 AND LineOfBusinessID < 1999)
BEGIN
	UPDATE [dbo].[EmailLU] SET [DefaultOn] = 1;
END

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[WorkspaceEmailXREF]') AND type in (N'U'))
BEGIN
	--Create new table
	CREATE TABLE [dbo].[WorkspaceEmailXREF] (
		WorkspaceEmailID int IDENTITY(1,1) not null,
		[EmailID] int not null,
		[WorkspaceID] int not null,
		[TurnOn] bit not null,
		UpdateDT datetime2 not null
	CONSTRAINT [PK_WorkspaceEmailXREF] PRIMARY KEY NONCLUSTERED 
	(
		WorkspaceEmailID ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[WorkspaceEmailXREF] WITH CHECK ADD CONSTRAINT [FK_WorkspaceEmail_Email] FOREIGN KEY ([EmailID]) REFERENCES [dbo].[EmailLU]([EmailID]);
	ALTER TABLE [dbo].[WorkspaceEmailXREF] WITH CHECK ADD CONSTRAINT [FK_WorkspaceEmail_Workspace] FOREIGN KEY ([WorkspaceID]) REFERENCES [dbo].[Workspace]([WorkspaceID]);

END
GO
/*
		3/29/18 twilson3 - BOEJ-3035 - BOE Emails
		## END ##
*/

/*
		## START ##
		4/04/18 brunworg - BOEJ-3177 - Tiered % - DB & Backend
*/

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TieredPercentage' AND Object_ID = Object_ID(N'[dbo].[ProjectMap]'))
BEGIN
	ALTER TABLE [dbo].[ProjectMap]
	ADD [TieredPercentage] DECIMAL(4,1) NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TieredPercentage' AND Object_ID = Object_ID(N'[version].[ProjectMap]'))
BEGIN
	ALTER TABLE [version].[ProjectMap]
	ADD [TieredPercentage] DECIMAL(4,1) NULL;
END
GO
/*
		4/04/18 brunworg - BOEJ-3177 - Tiered % - DB & Backend
		## END ##
*/

/*
		## START ##
		4/13/18 pattoncr - BOEJ-3185 - DB Work (Sikorsky Legacy Resources)
*/

IF EXISTS(SELECT * FROM sys.columns WHERE Name = N'LegacyResource' AND Object_ID = Object_ID(N'[dbo].[ProjectMap]'))
	AND EXISTS(SELECT * FROM sys.columns WHERE Name = N'LegacyResource' AND Object_ID = Object_ID(N'[version].[ProjectMap]'))
BEGIN
	PRINT('>> Making Sikorsky DB Changes <<');

	-- Create SikorskyLegacyResource table and insert the data.
	IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SikorskyLegacyResource]') AND type in (N'U'))
	BEGIN
		-- Create the new table.
		CREATE TABLE [dbo].[SikorskyLegacyResource] (
			ID int IDENTITY(1,1) NOT NULL,
			ResourceID varchar(20) NOT NULL UNIQUE,
			Name varchar(125) NOT NULL,
			CONSTRAINT [PK_SikorskyLegacyResource] PRIMARY KEY 
			(
				[ID] ASC
			)
		);

		-- Insert the Legacy Resource data.
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4000W', 'iTAS STA Direct');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FI1', 'Management Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045MMM', 'Standards');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4051F', 'Sys Safety Ft Worth');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('7433BAF', 'ADV. PROGRAMS FINANCIAL CONTROL');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8303D', 'Program Finance');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8306E', 'VXX Finance - Direct');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8306H', 'HH-60W (CRH) DIRECT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8306I', 'DEV PROG DIRECT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8312T', 'CCAD Project Mgmt');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8313B', 'Project Management');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8313J1C', 'H-53 Project Mgmt - IPT Leads');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8313V1C', 'Canada Proj Mgmt - IPT Leads');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8323D', 'Programs PCOE Support');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8323F1A', 'Program Integ Group - EPM Core');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8323R', 'Government Programs Direct');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8323T1D', 'PP&C Group - PPC');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8355G1C', 'VH Project Mgmt - IPT Leads');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8355Z', '53K Finance');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8455B', 'CRH Program Mgmt');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8455B1C', 'CRH Program Mgmt - IPT Leads');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8502A', 'Black Hawk Programs ILS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8502C', 'Maritime Program ILS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8502D', 'VH Program ILS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8502E', 'Heavy Lift Program ILS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8502O', 'Int''s Programs Joint Ventures');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8503X', 'SIMS Supply Support - Contract Projects');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8505U', 'TUHP Project MGMT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9209F', 'O&R VH-3D MATERIAL SPECIALIST');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045DD1', 'DTC & Affordability');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143OHB', 'Dynamic Components Other');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FTE', 'Avionics Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FYE', 'Aircraft Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143OF1', 'Florida Test Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143OF8', 'Florida Test Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('ICD-ODC', 'Indirect Charging Direct');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('SAC FREIGHT', 'Sikorsky Outgoing Freight');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9211B', 'SIMS Integrated Product Team');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9225C', 'Supply Mgmt-Sal-Dyn Components 02');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9210V', 'Supply Mgmt ASC');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9225A', 'Supply Mgmt Blades');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9225E', 'SUPPLY MGMT STRUCTURES');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9225F', 'Supply Mgmt - Sal - Dyn Components');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9225K', 'Delivery Assurance');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9225V', 'Supply Mgmt Strategic Sourcing');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABBKS', 'Backshop Labor calculated hours input to Prima Vera or ProPricer.  No Packaging or Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABAST', 'Assembly and Test Hours input from Prima Vera or Direct.  No Packaging or Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABFLAFO', 'FAFO Labor calculated dollars input from Prima Vera or ProPricer.  No Packaging or Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABBKS01H', 'Backshop Labor hours from SAP estimating.  No Packaging or Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABBKS02H', 'Backshop Labor hours from SAP estimating.  Packaging applied, no tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4049B', 'Aviation and Product Safety');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4049F', 'SAS Program Quality Support');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4049G', 'PROCESS QUALITY ENGINEER');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4050D', 'Systems Safety - HSV');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVY', 'Aircraft Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4041TTG', 'ELECT. AND UTILITY SYSTEMS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4041TTH', 'AVIONICS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4041TTJ', 'ASI PROD. LINE LEADS/STAFF');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4041TTL', 'AVIONICS SCIENCES ENGINEERING');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043EYE', 'Airframe Design - WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043EYK', 'Harness Eng - WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043EYL', 'Component Installation - WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043EYM', 'Propulsion - WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043FHG', 'Airframe Structural Analysis - WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043PP3', 'WPB Pilots');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043SWM', 'Instrumentation Eng');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043SWS', 'Instrumentation Design');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043SWT', 'Instrumentation Tech');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043VTB', 'Avionics System Integration WPB Direct');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043VTD', 'ELECT. FLT CTRL SYS. WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043VTE', 'HYDRAULIC/MECH. FLT CONTROLS WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8305A', 'CONTRACT ADMINISTRATION');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9210A', 'Supply Mgmt Corporate');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABTOL', 'Tooling Hours input from Prima Vera or Direct.  No Packaging or Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4047BSL', 'Trng-Instr/SME');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4047BSO', 'Trng-Mgt Ops');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('0010', 'DEV.FLT.TEST CTR (OPER)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('0020', 'DEV.FLT.TEST CTR QA');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('0340', 'TOOL ROOM');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4010Z', 'Fleet Safety');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045A', 'ASO - WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FMM', 'Fort Worth DMSMS and Standards');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4146IHA', 'DYNAMICS BLADE ENGINEERING - INDIANA');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4146KHA', 'Blades Eng (KY)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVT', 'Advanced Manufacturing');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4350S', 'S92 ENGR MGT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4460XAE', 'VXX Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4745A', 'HUNTVILLE PROGRAM MANAGEMENT OFFICE');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('5050', 'WPB MACHINE SHOP');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8303M', 'INT''L DIRECT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8303O', 'Canada MHP Financial Control - Direct');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8313A', 'NAVAL HAWK PROJECT MANAGEMENT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8313C', 'EXEC TRANSPORT PROJECT MANAGEMENT GROUP');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8313N', 'Maritime Afmkt Prog Mgmt');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8313V', 'CANADA PROJECT MANAGEMENT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8323F', 'Program Integration Group');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8323T', 'PP&C Group');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8325G', 'International Programs');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8325M', 'INDUSTRIAL COOPERATION');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8327H', 'International Programs Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8355G', 'VXX Project Management');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8502E1C', 'ILS HEAVY LIFT & MATURE PRGM - IPT Leads');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8502G', 'USG Army Offsite');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8502I', 'Navy Aftmkt Proj Mgt');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8503B', 'SIMS - Contract Projects');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8503Z', 'USG Maritime Aftmkt Offsite');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8504F', 'ILS Support');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9209G', 'O&R PROGRAM MANAGEMENT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9209T', 'O&R MANAGEMENT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9210F', 'MATERIAL OPERATIONS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABOPF', 'Operations Florida Labor calculated dollars input from Prima Vera or ProPricer.  No Packaging or Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9225B', 'Supply Mgmt Procurement Services');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('9225G', 'SUPPLY MGMT DEVELOPMENT PROGRAM');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8303A', 'USG PRICING & PROPOSAL MANAGEMENT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('A130L', 'SSSI QA Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('A140L', 'SSSI Mult Func Mfg Asc Mgr');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('A230L', 'SSSI Mult Func Mfg Planner Sup');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('A300L', 'SSSI Technician Field Sr Spec');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('A380L', 'SSSI Technician Field Sr');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('A390L', 'SSSI Technician Field Spec');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('A590L', 'SSSI Aircraft Mechanic');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('B110L', 'SSSI Sys Engr-Field Tech Spt Asc');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('B120L', 'SSSI Sys Engr-Field Tech Support');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('B130L', 'SSSI Sys Engr-Field Tech Spt Sr');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('B140L', 'SSSI Sys Engr-Field Tch Spt Asc Mgr');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('B220L', 'SSSI Items Analyst');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('B230L', 'SSSI Program/Customer Rep Sr');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8402D', 'VXX Program');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8325A', 'Black Hawk Program Management');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('SAC CSP', 'Sikorsky Commercial Sell Price');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045QI1', 'Eng. ITC - Direct');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8455A1B', 'CRH Core Program Management');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8326V', 'DSS USG Bus Dev');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8313E', 'Aftermarket Est & Pricing');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('8303Z', 'SAS Business Operations SPI');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('LSTRAVEL', 'Unescalated Travel');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABAST01H', 'Assembly and Test Labor hours from SAP estimating.  No Packaging or Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABAST02H', 'Assembly and Test Labor hours from SAP estimating.  Packaging applied, no tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABAST03H', 'Assembly and Test Labor hours from SAP estimating.  Tooling applied, no packaging applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABAST04H', 'Assembly and Test Labor hours from SAP estimating. Packaging and Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABBKS03H', 'Backshop Labor hours from SAP estimating.  Tooling applied, no packaging applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABBKS04H', 'Backshop Labor hours from SAP estimating. Packaging and Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABFLAFO01H', 'FAFO Labor hours from SAP estimating.  No Packaging or Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABFLAFO02H', 'FAFO Labor hours from SAP estimating.  Packaging applied, no tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABFLAFO03H', 'FAFO Labor hours from SAP estimating.  Tooling applied, no packaging applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABFLAFO04H', 'FAFO Labor hours from SAP estimating. Packaging and Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABOPF01H', 'Operations Florida Labor hours from SAP estimating.  No Packaging or Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABOPF02H', 'Operations Florida Labor hours from SAP estimating.  Packaging applied, no tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABOPF03H', 'Operations Florida Labor hours from SAP estimating.  Tooling applied, no packaging applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045QAJ', 'Core Eng. ITC - Direct');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043VTH', 'AVIONICS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043VTL', 'AVIONICS SCIENCES ENGINEERING WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043WWB', 'Flight Test Structures/Dynamics');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043WWC', 'Flight Test Special Projects');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043WWD', 'Flight Test Performance');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043WWE', 'Flight Test Avionics/Electrical');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043WWF', 'WPB Flight Test');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043WWG', 'Flight Test Flight Controls/HQ');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043WWI', 'Engineering Management WPB Direct');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043WWO', 'Flight Test Data Acquisition');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043WWR', 'Flight Test Telemetry Ops');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4044CSB', 'Analytics (TX)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4044JSC', 'PLM/LEM (Huntsville, AL)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4044JSD', 'LSA (Huntsville, AL)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150GYD', 'Design Quality Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HSB', 'HUNT - Analytics');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4650KSB', 'Analytics CT SB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4755NSB', 'Analytics NY SB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045MML', 'Configuration Mgmt Mfg Bill of Material');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4650GSR', 'Field Support Texas');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4650MSR', 'Field Support CT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4044JSG', 'Flight Manuals (Huntsville, AL)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4044MSI', 'Training (VA)- IPT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045DD2', 'Operations Support');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045DD4', 'Strategic Sourcing');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045DD6', 'Affordability');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045EE1', 'APPS. Development');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045EE2', 'Standard Work');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045EE3', 'Digital Product Definition (DMU)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABOPF04H', 'Operations Florida Labor hours from SAP estimating. Packaging and Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABORB', 'Overhaul and Repair Connecticut Labor Direct Input. Component only - not subject to application of O&R Vendor Processing CER');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABORC', 'Overhaul and Repair Connecticut Labor Direct Input. Component only - subject to application of O&R Vendor Processing CER');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABORC01H', 'Overhaul and Repair Connecticut Labor hours from SAP estimating.  No Packaging or Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABORC02H', 'Overhaul and Repair Connecticut Labor hours from SAP estimating.  Packaging applied, no tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABORC03H', 'Overhaul and Repair Connecticut Labor hours from SAP estimating.  Tooling applied, no packaging applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABORC04H', 'Overhaul and Repair Connecticut Labor hours from SAP estimating. Packaging and Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABTOL01H', 'Tooling Labor hours from SAP estimating. No Packaging and Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABTOL02H', 'Tooling Labor hours from SAP estimating.  Packaging only applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABTOL03H', 'Tooling Labor hours from SAP estimating.  No Packaging applied,  tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABTOL04H', 'Tooling Labor hours from SAP estimating.  Tooling applied,  packaging applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABTRY', 'Troy Manufacturing Labor calculated dollars from SAP estimating. No Packaging and Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABTRY01H', 'Troy Manufacturing Labor hours from SAP estimating. No Packaging and Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABTRY02H', 'Troy Manufacturing Labor hours from SAP estimating.  Packaging applied no Tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABTRY03H', 'Troy Manufacturing Labor hours from SAP estimating.  No Packaging applied,  tooling applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('HLABTRY04H', 'Troy Manufacturing Labor hours from SAP estimating.  Tooling applied,  packaging applied.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('RMS-ODC', 'RMS Other Value Add');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045EE5', 'Process Management');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045FF1', 'GROUND TEST');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045FF2', 'FLIGHT TEST STFD');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045FF3', 'INSTRU. STFD');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045FF4', 'MAT.& PROC. LAB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4041TTA', 'SIMULATION');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4041TTB', 'Crew Station Design');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4041TTC', 'SOFTWARE');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4041TTD', 'ELECT.FLT CTRL SYS.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4041TTE', 'HYDRAULIC/MECH. FLT CONTROLS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045FF5', 'GRD TEST LABS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045FF6', 'ELEC. FLT CNTRL/SIM. LABS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045FF7', 'INSTRU. LABS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045FF8', 'TEST ENG. PROD. LINE LEADS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGA', 'SYSTEM ENGR STAFF');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGB', 'AERODYN.& EXT. ACOUSTICS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGC', 'HANDLING QUAL./SIM.');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGD', 'DYNAM./ACOUS.FP&T SUPPORT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGE', 'EXPERIMENTAL AEROMECHAN  ICS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGF', 'DYNAM.& INTERNAL ACOUSTICS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGG', 'LOADS&CRIT./SURVIVABILITY');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGH', 'Thermal Mgmt');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGK', 'MASS PROPERTIES');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGM', 'FLT DYNAM. SIMULATION');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGN', 'Susceptibility');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGO', 'Vulnerability');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045GGT', 'AIRCRAFT ATTRIBUTES');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045HHA', 'BLADE ENGINEERING');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045HHB', 'ROTORS ENGINEERING');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045HHC', 'TRANSMISSIONS ENGINEERING');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045HHG', 'Airframe Structural Analysis');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045HHH', 'Dynamic Systems Structural Analysis');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045HHZ', 'Dynamic Systems Leads/Tech Fellows');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FHG', 'Airframe Structural Analysis - Ft Worth');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FHH', 'Dynamic Systems Structural Analysis - Ft Worth');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FM1', 'Fort Worth Texas PRELIMINARY DESIGN');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FMS', 'Fort Worth Texas SYSTEM ENGINEERING');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FMV', 'Fort Worth Texas CONFIGURATION MANAGEMENT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FSM', 'Reliability & Maintainability - Ft Worth');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FSN', 'Diagnostics - Ft Worth');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FTC', 'FORT WORTH SOFTWARE');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FTH', 'F WORTH AVIONICS SC');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FTJ', 'Fort Worth Texas ASI PROD LEADS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FTL', 'Fort Worth Texas Avionics Sciences Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FVD', 'ME M&P - Ft Worth');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HAE', 'Huntsville MANAGEMENT STAFF/CHIEF ENGINEERS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HF8', 'HUNT TEST ENG PRD SC');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HHG', 'Airframe Structural Analysis - Huntsville');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HMS', 'Huntsville SYSTEM ENGINEERING');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HMV', 'Huntsville CONFIGURATION MANAGEMENT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HSM', 'Reliability & Maintainability - Huntsville');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HTB', 'Huntsville CREW STATION DESIGN');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HTC', 'Huntsville SOFTWARE');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HTG', 'Huntsville ELECT. AND UTILITY SYSTEMS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HTH', 'Huntsville AVIONICS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HTJ', 'HUNTSVILLE ASI PROD LEADS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HVD', 'ME M&P - Huntsville');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HVF', 'ME Aerostructure - Huntsville');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HYE', 'HUNT AIRFRAME DES & SC');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HYK', 'HUNT HARNESS ENG SC');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046HYL', 'HUNT COMP INTEGR SC');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4048TC1', 'Airworthiness Certification');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4048TC2', 'Airworthiness Safety');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4048VSI', 'Training (AL)- Training IPT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4048VSK', 'Training (AL)- Instructional Design');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4146ID4', 'ENG INDIANA - DIRECT - DC SUPPORT ');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4146IHC', 'Indiana TRANSMISSIONS ENGINEERING');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4146IHG', 'Airframe Structural Analysis - Indiana');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4146KD4', 'ENG KENTUCKY - DIRECT - DC SUPPORT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4146KHG', 'Airframe Structural Analysis - Kentucky');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4146THG', 'Airframe Structural Analysis - Troy');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4146TVF', 'ME Aerostructure - Troy');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4146TVU', 'ME MRB - Troy');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4147KP1', 'Coatesville   PILOTS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4147PAE', 'Pax River MANAGEMENT ENGINEERING');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVA', 'ME Blades');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVB', 'ME Dynamic Components');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVD', 'ME M&P');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4350SAE', 'ENG MGT AE - S92');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4450DAE', 'SMS Army - Huntsville Chiefs');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4450PAE', 'Engineering Management Chiefs SMS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4650ASN', 'Health Management Systems');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4650CSM', 'Reliability & Maintainability');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4650HSY', 'Product Support Eng Leads');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4650JSZ', 'ITAS Engineering IPT Chiefs');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4750NSN', 'Impact Tech NY');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4751NSN', 'Impact Tech NY 120');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4755NSN', 'Impact Tech PA');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045IYZ', 'AIR VEHICLE DES. PROD.LEADS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045MM1', 'ADV. Engr - ADVANCED DESIGN AND BUSINESS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045MM2', 'Engineering Estimating');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045MMN', 'Systems Software Configuration Management');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045MMO', 'CM - Proc Mgmt');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045MMS', 'SYSTEM ENGINEERING');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045MMV', 'CONFIGURATION MANAGEMENT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045MMX', 'System Security Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045MMY', 'System Software Quality Assurance');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045NN1', 'Operations Analysis');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045NN4', 'Operations Analysis');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045NN6', 'Advanced Programs - Direct');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4045PP1', 'PILOTS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046AAE', 'MANAGEMENT STAFF/CHIEF ENGINEERS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FAE', 'Fort Worth Texas MANAGEMENT STAFF/CHIEF ENGINEERS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FE3', 'Fort Worth Texas DIGITAL PRODUCT DEFINITION (DMU)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FGC', 'Fort Worth Texas HANDLING QUALITIES/CONTROL LAWS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FGF', 'Fort Worth Texas DYNAMICS & INTERNAL ACOUSTICS');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FGK', 'Fort Worth Texas MASS PROPERTIES');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FHA', 'F WORTH BLADE ENG SC');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FHB', 'F WORTH ROTOR ENG SC');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4046FHC', 'F WORTH TRANS ENG SC');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4047ASB', 'Analytics (CT)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4047BSC', 'PLM/LEM (CT)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4047BSD', 'LSA (CT)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4047BSF', 'Provisioning (CT)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4047BSG', 'Flight Manuals (CT)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4047BSH', 'Ground Support Equipment (CT)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4047BSI', 'Training - IPT CT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4047BSK', 'Trng-Inst Dsgn/Crswr');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4047BSP', 'ISS Group CT');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4047BSQ', 'Library/Distribution');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143AWL', 'Flight Test Management');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143BWK', 'Parts Mgmt/Control');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143BWP', 'CHIEFS OF FLIGHT TEST');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143FXA', 'DFC AC Managers');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143FXB', 'DFC Build Managers');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143OGW', 'ENG Sciences - WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143OMS', 'System Engineering WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143OMV', 'Configuration Mgmt (WPB)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143ONW', 'WPB Advanced Programs');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4143OSM', 'Reliability & Maintainability - WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVE', 'ME M&P Labs');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVF', 'ME Aerostructure');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVK', 'ME Harness');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVL', 'ME Component');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVP', 'ME Tool Design');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVR', 'ME AFO');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVU', 'ME MRB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150AVZ', 'ME Leads/Tech Fellows');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150GYE', 'Airframe Design  ');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150GYH', 'Airframe Development');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150GYJ', 'Mass');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150GYM', 'Propulsion');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150GYY', 'Landing Gear Design');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150WYK', 'Harness Engineering');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4150WYL', 'Component Installation');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4343EVR', 'ME AFO - WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4343EVU', 'ME MRB - WPB');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4043EYZ', 'WPB Airframe Management');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('1340', 'Tooling Fabrication');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('4010ZB1', 'Field and Production Safety');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('TRAVEL4', 'Travel Baggage');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('B240L', 'SSSI Items Analyst Stf');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('B300L', 'SSSI Technician Field Sr Spec');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('B390L', 'SSSI Technician Field Spec');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('B580L', 'SSSI Aircraft Mechanic UH (60/1)');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('BW30L-15-4527-23023', 'SSSI Aircraft Mechanic III');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('CW00L-15-4527-23380', 'SSSI Ground Support Equip Mechanic');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('CW20L-15-4527-23022', 'SSSI Aircraft Mechanic II');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('CW30L-15-4527-23023', 'SSSI Aircraft Mechanic III');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('LODC', 'Other SSSI ODC');
		insert into SikorskyLegacyResource (ResourceId, Name) values ('CW20L-15-4527-23382', 'SSSI Ground Support Equip Worker');
	END

	-- Add the LegacyResourceId columns
	IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'LegacyResourceID' AND Object_ID = Object_ID(N'[dbo].[ProjectMap]'))
		ALTER TABLE [dbo].[ProjectMap] ADD LegacyResourceID int NULL FOREIGN KEY REFERENCES SikorskyLegacyResource(ID);
	IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'LegacyResourceID' AND Object_ID = Object_ID(N'[version].[ProjectMap]'))
		ALTER TABLE [version].[ProjectMap] ADD LegacyResourceID int NULL;

	-- This is needed to make this code executable multiple times. Otherwise it'll fail to compile, because LegacyResource column is invalid
	EXEC sp_executesql N'
		-- Check to make sure we have a mapping for all LegacyResource values before performing the migration.  If not, we want to abort the migration and output the problems that need to be resolved before continuing.
		IF (NOT EXISTS(
			select distinct LegacyResource, WorkspaceId from [dbo].[ProjectMap] p left join SikorskyLegacyResource s on (p.LegacyResource = s.ResourceID) where p.LegacyResource is not null and s.ResourceID is null
			union
			select distinct LegacyResource, WorkspaceId from [version].[ProjectMap] p left join SikorskyLegacyResource s on (p.LegacyResource = s.ResourceID) where p.LegacyResource is not null and s.ResourceID is null
		))
		BEGIN
			PRINT(''Sikorsky Migration Executing'');

			-- Create the new mappings to the proper IDs for LegacyResources.
			UPDATE [dbo].[ProjectMap] set LegacyResourceID = s.ID from [dbo].[ProjectMap] p join SikorskyLegacyResource s on (p.LegacyResource = s.ResourceID) where p.LegacyResource is not null;
			UPDATE [version].[ProjectMap] set LegacyResourceID = s.ID from [version].[ProjectMap] p join SikorskyLegacyResource s on (p.LegacyResource = s.ResourceID) where p.LegacyResource is not null;
			
			-- Drop the LegacyResource columns from the ProjectMap table and the corresponding version table.
			IF EXISTS(SELECT * FROM sys.columns WHERE Name = N''LegacyResource'' AND Object_ID = Object_ID(N''[dbo].[ProjectMap]''))
				ALTER TABLE [dbo].[ProjectMap] DROP COLUMN LegacyResource;
			IF EXISTS(SELECT * FROM sys.columns WHERE Name = N''LegacyResource'' AND Object_ID = Object_ID(N''[version].[ProjectMap]''))
				ALTER TABLE [version].[ProjectMap] DROP COLUMN LegacyResource;
		END
		ELSE
		BEGIN
			select distinct ''No Mapping'' as ''Error'', ''[dbo].[ProjectMap]'' as ''Table'', LegacyResource, WorkspaceId from [dbo].[ProjectMap] p left join SikorskyLegacyResource s on (p.LegacyResource = s.ResourceID) where p.LegacyResource is not null and s.ResourceID is null
			union
			select distinct ''No Mapping'' as ''Error'', ''[version].[ProjectMap]'' as ''Table'', LegacyResource, WorkspaceId from [version].[ProjectMap] p left join SikorskyLegacyResource s on (p.LegacyResource = s.ResourceID) where p.LegacyResource is not null and s.ResourceID is null;

			RAISERROR (''Values have been found in the [dbo].[ProjectMap].[LegacyResource] and/or [version].[ProjectMap].[LegacyResource] columns that have no corresponding mapping in the [dbo].[SikorskyLegacyResource].[ResourceId] column.  Please see the "Results" tab and resolve the issue before continuing.'', 11, 1);
		END
	';
END
ELSE
BEGIN
	PRINT '>> Sikorsky work was done before, or there was an issue w/ column LegacyResource. <<';
END
GO

/*
		4/13/18 pattoncr - BOEJ-3185 - DB Work (Sikorsky Legacy Resources)
		## END ##
*/
/*
		## START ##
		4/16/18 twilson3 - BOEJ-3202, BOEJ-3184 New SSRS Reports
*/

IF(NOT EXISTS(SELECT 1 FROM [dbo].[ReportLU] WHERE ReportID = 36))
BEGIN
	INSERT INTO [dbo].[ReportLU] (ReportID, ReportName, Description)
	VALUES (36, 'Flattened Costs by CLIN, Res, Act & Yr (yrs)', 'Cost sorted by CLIN, activity, resource, pricing code and year for an 8 year period'),
	(37, 'Offload Detailed Report', 'Detailed summary by year of the offload calculation for each new offload activity generated in BOEDB and added to the post-offload project map');
END
GO

/*
		4/16/18 twilson3 - BOEJ-3202, BOEJ-3184 New SSRS Reports
		## END ##
*/