/*
	### DO NOT EXECUTE AS A PART OF ANY RELEASE ###
	
	This Script will setup an application admin and a default test workspace. It is meant for GenBOE database.
	
*/

-- The application will not work if any of these fields do not match AD
DECLARE @ntid VARCHAR(50) = '';
DECLARE @displayName VARCHAR(50) = '';
DECLARE @firstName VARCHAR(50) = '';
DECLARE @lastName VARCHAR(50) = '';
DECLARE @email VARCHAR(50) = '';
-- NOTHING ELSE TO CHANGE BEYOND THIS LINE

-- Insert the user
INSERT INTO [ETIUser] (UpdateDT, NTID, DisplayName, FirstName, LastName, EmailAddress, IsUsPerson, IsSubcontractor) VALUES (GETDATE(), @ntid, @displayName, @firstName, @lastName, @email, 1, 0);

-- Grant System Admin role
INSERT INTO [SystemUserRole] (UpdateDT, ETIUserID, RoleId) VALUES (GETDATE(), (SELECT TOP(1) EtiUserId FROM EtiUser), 6);

-- Setup a default WS
IF(NOT EXISTS(SELECT 1 FROM dbo.Workspace WHERE WorkspaceId = 1))
BEGIN
	SET IDENTITY_INSERT dbo.Workspace ON;
	DECLARE @userId INT = (SELECT TOP 1 EtiUserId FROM dbo.ETIuser);
	
	INSERT INTO dbo.Workspace
		 ([WorkspaceID],[UpdateDT],[WorkspaceName],[WorkspaceShortName],[WorkspaceStateID],[ContractStartDate],[ContractEndDate],[ProposalSubmitDate]
		  ,[WorkspaceDescription],[CostVolumeLeadPricerUserID],[RFPNumber],[TemplateID],[ContainsOCI],[CreatedByETIUserID],[AllowSearch]
		  ,[ResourceListID],[PerformingOrganizationListID],[PerformingOrganizationChangeFlag],[TrackingNumber],[ContainsTemplate],[NumProPricerExport]
		  ,[ProposalStatusID],[StatusComment],[BOEExportSortByID]
		  ,[SegmentID],[LineOfBusinessID],[ProposalClassID]
		  ,[ProposalTitle],[IsDeleted],[DateDeleted],[ResourcePrecision],[RecalculationStartedDate],[CostPrecision],[IsUsingEquivalentPerson])
	  VALUES (1, GETDATE(), 'Test - CIO', 'test_-_cio', 2, '2013-12-15', '2016-12-15', NULL, 'Test Workspace for EBS CIO.', @userId, NULL, 9001, 0, @userId, 1, 1, 1, 0, NULL, 0, 0,
		(SELECT TOP 1 ProposalStatusID FROM ProposalStatusLU), NULL, (SELECT TOP 1 SortByID FROM SortByLU),
		(SELECT TOP 1 SegmentId FROM SegmentLU),(SELECT TOP 1 LineOfBusinessID FROM LineOfBusiness), (SELECT TOP 1 ProposalClassID FROM ProposalClassLU),
		NULL, 0, NULL, NULL, NULL, 2, 0);
		
	SET IDENTITY_INSERT dbo.Workspace OFF;
END