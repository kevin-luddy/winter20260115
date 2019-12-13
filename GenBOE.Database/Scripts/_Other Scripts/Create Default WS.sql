IF(NOT EXISTS(SELECT 1 FROM dbo.Workspace WHERE WorkspaceId = 30000))
BEGIN
SET IDENTITY_INSERT dbo.Workspace ON
INSERT INTO dbo.Workspace
	 ([WorkspaceID],[UpdateDT],[WorkspaceName],[WorkspaceShortName],[WorkspaceStateID],[ContractStartDate],[ContractEndDate],[ProposalSubmitDate]
      ,[WorkspaceDescription],[CostVolumeLeadPricerUserID],[RFPNumber],[TemplateID],[ContainsOCI],[CreatedByETIUserID],[AllowSearch]
      ,[ResourceListID],[PerformingOrganizationListID],[PerformingOrganizationChangeFlag],[TrackingNumber],[ContainsTemplate],[NumProPricerExport]
      ,[ProposalStatusID],[StatusComment],[BOEExportSortByID],[SegmentID],[LineOfBusinessID]
      ,[ProposalClassID],[ProposalTitle],[IsDeleted],[DateDeleted],[ResourcePrecision],[RecalculationStartedDate],[CostPrecision]
	  ,[IsUsingEquivalentPerson])
  VALUES (1 ,GETDATE(), 'Test - CIO', 'test_-_cio', 2, '2013-12-15', '2016-12-15', NULL, 'Test Workspace for EBS CIO.', 1006, 3, NULL, 9001,
   (SELECT TOP 1 EtiUserId FROM dbo.ETIuser), 1, 2, 2, 1, NULL, 0, 14, 0, NULL, 1, 1, 1001, 1008, 1001, NULL, NULL, NULL, NULL, 2, 0)
SET IDENTITY_INSERT dbo.Workspace OFF
END
