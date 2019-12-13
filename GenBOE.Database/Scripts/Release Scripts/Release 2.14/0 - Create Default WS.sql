IF(NOT EXISTS(SELECT 1 FROM dbo.Workspace WHERE WorkspaceId = 30000))
BEGIN
SET IDENTITY_INSERT dbo.Workspace ON
INSERT INTO dbo.Workspace
	 ([WorkspaceID],[UpdateDT],[WorkspaceName],[WorkspaceShortName],[WorkspaceStateID],[ContractStartDate],[ContractEndDate],[ProposalSubmitDate]
      ,[WorkspaceDescription],[ProposalTypeID],[CostVolumeLeadPricerUserID],[RFPNumber],[TemplateID],[ContainsOCI],[CreatedByETIUserID],[AllowSearch]
      ,[ResourceListID],[PerformingOrganizationListID],[PerformingOrganizationChangeFlag],[TrackingNumber],[ContainsTemplate],[NumProPricerExport]
      ,[ProposalStatusID],[StatusComment],[DTSAutoCalculateID],[CalculateLaborCostFlag],[ProductLineID],[BOEExportSortByID],[SegmentID],[LineOfBusinessID]
      ,[ProposalClassID],[ProposalTitle],[IsDeleted],[DateDeleted],[ResourcePrecision],[RecalculationStartedDate],[RestrictDTS] ,[CostPrecision]
	  ,[SummaryBoeSelection] ,[SummaryBoeCustomFieldSelection] ,[IsUsingEquivalentPerson])
  VALUES (30000,GETDATE(), 'Test - CIO', 'test_-_cio', 2, '2013-12-15', '2016-12-15', NULL, 'Test Workspace for EBS CIO.', 1006, 3, NULL, 9001, 0,
   (SELECT TOP 1 EtiUserId FROM dbo.ETIuser), 1, 2, 2, 1, NULL, 0, 14, 0, NULL, 1, 0, NULL, 1, 1001, 1008, 1001, NULL, NULL, NULL, NULL, NULL, 0, 2, 2, NULL, 0)
SET IDENTITY_INSERT dbo.Workspace OFF
END
