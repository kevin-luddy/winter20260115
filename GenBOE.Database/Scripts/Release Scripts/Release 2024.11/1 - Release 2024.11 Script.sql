EXEC[dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.11';
GO

/*
	## START ##

	8/8/2024 [ranzalon] - SLMX_POLM_PROPH-1623: Purge Old Data
*/

-- Purge old users who are not connected to any tables
DELETE u
  FROM [dbo].[ETIuser] u
  WHERE
	NOT EXISTS
	(
		SELECT 1
		FROM WorkspaceUserXREF wux
		WHERE u.ETIUserID = wux.ETIUserId
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM WorkspaceStateHistory wsh
		WHERE u.ETIUserID = wsh.ChangedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM WorkspaceUserRole wur
		WHERE u.ETIUserID = wur.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM WorkspaceVersion wv
		WHERE u.ETIUserID = wv.CreatedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM PerDiem pd
		WHERE u.ETIUserID = pd.PerDiemLastUpdateETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM RteTemplate rt
		WHERE u.ETIUserID = rt.AuthorID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM BOEApproval ba
		WHERE u.ETIUserID = ba.ApprovalETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM BOEApprovalHistory bah
		WHERE u.ETIUserID = bah.ApprovalETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM BOEComment bc
		WHERE u.ETIUserID = bc.BOECommentETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM BOECommentHistory bch
		WHERE u.ETIUserID = bch.ChangedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM BOEPotentialRole bpr
		WHERE u.ETIUserID = bpr.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM BOEStateHistory bsh
		WHERE u.ETIUserID = bsh.ChangedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM BOEUserRole bur
		WHERE u.ETIUserID = bur.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM BOEUserRoleHistory burh
		WHERE u.ETIUserID = burh.CurrentETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM BOEUserRoleHistory burh
		WHERE u.ETIUserID = burh.UpdatedETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM BOEUserRoleHistory burh
		WHERE u.ETIUserID = burh.ChangedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM SystemUserRole sur
		WHERE u.ETIUserID = sur.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM Trip t
		WHERE u.ETIUserID = t.FareLastUpdateETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM MessageConfirmation mc
		WHERE u.ETIUserID = mc.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM Workspace w
		WHERE u.ETIUserID = w.CostVolumeLeadPricerUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM Workspace w
		WHERE u.ETIUserID = w.CreatedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM Location l
		WHERE u.ETIUserID = l.UpdatedByETIUserID
	)

GO

-- Purge Output Format Templates that are archived and unused
  DELETE FROM [dbo].[OutputFormatTemplate]
  WHERE TemplateID IN (  
	  SELECT o.TemplateID
	  FROM [dbo].[OutputFormatTemplate] o
	  LEFT JOIN Workspace w ON o.TemplateID = w.TemplateID
	  WHERE o.IsActive = 0
	  GROUP BY o.TemplateID, o.Template
	  HAVING COUNT(w.TemplateID) = 0
  )

GO

-- Purge old Report XML Data (also run via Data Cleanup script)
DELETE
  FROM [dbo].[ReportXmlData]
  WHERE UpdateDT <= DATEADD(DAY, -1, GETDATE())

GO

-- Drop unused Stored Procedures
IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getLaborType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getLaborType];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getLaborSpread]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getLaborSpread];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getBOEPermissions]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getBOEPermissions];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteODCSpreadByODCTypeID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteODCSpreadByODCTypeID];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteAllODCTaskElements]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteAllODCTaskElements];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteODCTaskElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteODCTaskElement];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deletePerDiem]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deletePerDiem];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspace]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspace];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceRMSTravelEscalationRate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceRMSTravelEscalationRate];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[deleteWorkspaceRMSTravelNonzoneFeesAndCosts]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[deleteWorkspaceRMSTravelNonzoneFeesAndCosts];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getOrdinaryVariableByTaskElementID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getOrdinaryVariableByTaskElementID];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspaceHistoryLog]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspaceHistoryLog];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspacePerformingOrganizationInUseFlagByPerformingOrganizationID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspacePerformingOrganizationInUseFlagByPerformingOrganizationID];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspacePermissions]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspacePermissions];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getWorkspaceVariableByTaskElementID]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getWorkspaceVariableByTaskElementID];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertBOECommentHistory]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertBOECommentHistory];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[insertOutputFormatTemplate]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[insertOutputFormatTemplate];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateWorkspaceStatus]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateWorkspaceStatus];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertODCSpread]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertODCSpread];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertODCTaskElement]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertODCTaskElement];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertODCType]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertODCType];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[upsertPerformingOrganization]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[upsertPerformingOrganization];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getConfigurationValue]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getConfigurationValue];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getBOEIDForProjectMapAdvancedSearch]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getBOEIDForProjectMapAdvancedSearch];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[getBOEIDForProjectMapQuickSearch]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[getBOEIDForProjectMapQuickSearch];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[loadForesightProgram]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[loadForesightProgram];

GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[updateWorkspaceAutoCalculateDTS]') AND type in (N'P', N'PC'))
	DROP PROCEDURE [dbo].[updateWorkspaceAutoCalculateDTS];

GO

/*
	8/8/2024 [ranzalon] - SLMX_POLM_PROPH-1623: Purge Old Data

	## END ##
*/