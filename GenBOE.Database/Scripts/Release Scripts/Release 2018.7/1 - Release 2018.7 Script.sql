EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2018.7';
GO

/*
		## START ##
		7/24/18 RJ - BOEJ-3662: Update Email Triggers
*/

IF NOT EXISTS (SELECT 1 FROM [dbo].[EmailLU] WHERE [EmailID] = 44)
BEGIN

INSERT INTO [dbo].[EmailLU] 
VALUES (44, 'genBOE: Workspace Status Changed from Working to Locked', 'The Workspace that the following BOE belongs to has changed status from Working to Locked. The Author is no longer able to edit Travel Rates, Labor Rates, and Hour and Cost estimates in the BOE. Text is still editable for BOEs in Draft. <BR/><BR/>Workspace/Proposal: {0}<BR/>WBS: {1} {2}<BR/>BOE Title: {9}<BR/>CLIN: {3} {4}<BR/>BOE: {5}<BR/>Author: {6} <BR/> Approver(s): {7}<BR/><BR/>{8}', 'Workspace Status changed from Working to Locked', 1, 0, 'Workflow', 'BOE Author(s)'),
(45, 'genBOE: Workspace Status Changed from Locked to Working', 'The Workspace that the following BOE belongs to has changed status from Locked to Working. The BOE is once again open for edits. The Author must submit the BOE to the Approver(s) once it is ready for approval.  <BR/><BR/>Workspace/Proposal: {0}<BR/>WBS: {1} {2}<BR/>BOE Title: {9}<BR/>CLIN: {3} {4}<BR/>BOE: {5}<BR/>Author: {6} <BR/> Approver(s): {7}<BR/><BR/>{8}', 'Workspace Status Changed from Locked to Working', 1, 0, 'Workflow', 'BOE Author(s)'),
(46, 'genBOE: Workspace Status Changed from Working to Initialization', 'The Workspace that the following BOE belongs to has changed status from Working to Initialization. The Author is no longer able to edit the BOE. <BR/><BR/>Workspace/Proposal: {0}<BR/>WBS: {1} {2}<BR/>BOE Title: {9}<BR/>CLIN: {3} {4}<BR/>BOE: {5}<BR/>Author: {6} <BR/> Approver(s): {7}<BR/><BR/>{8}', 'Workspace Status changed from Working to Initialization', 1, 0, 'Workflow', 'BOE Author(s)'),
(47, 'genBOE: Workspace Status Changed from Initialization to Working', 'The Workspace that the following BOE belongs to has changed status from Initialization to Working. The BOE is once again open for edits. The Author must submit the BOE to the Approver(s) once it is ready for approval.  <BR/><BR/>Workspace/Proposal: {0}<BR/>WBS: {1} {2}<BR/>BOE Title: {9}<BR/>CLIN: {3} {4}<BR/>BOE: {5}<BR/>Author: {6} <BR/> Approver(s): {7}<BR/><BR/>{8}', 'Workspace Status changed from Initialization to Working (all subsequent times)', 1, 0, 'Workflow', 'BOE Author(s)');

END
GO

/*
		7/24/18 RJ - BOEJ-3662: Update Email Triggers
		## END ##
*/