EXEC[dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.10';
GO

/*
	## START ##

	8/8/2024 [ranzalon] - SLMX_POLM_PROPH-1623: Purge Old Data
*/

-- Remove Users from WorkspaceUserXREF who have not accessed a Workspace in over 2 years
DELETE FROM dbo.WorkspaceUserXREF
WHERE ETIUserID in
(
	SELECT ETIUserId 
	FROM dbo.WorkspaceUserXREF 
	GROUP BY ETIUserId 
	HAVING MAX(LastAccessed) < DATEADD(YEAR, -2, GETDATE())
)

-- Purge old users who are not connected to any tables
DELETE FROM [dbo].[ETIUser]
WHERE ETIUserID in
(
	SELECT ETIUserID
	FROM [dbo].[ETIUser] u
	WHERE
	NOT EXISTS
	(
		SELECT 1
		FROM dbo.WorkspaceUserXREF wux
		WHERE u.ETIUserID = wux.ETIUserId
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.WorkspaceStateHistory wsh
		WHERE u.ETIUserID = wsh.ChangedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.WorkspaceUserRole wur
		WHERE u.ETIUserID = wur.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.WorkspaceVersion wv
		WHERE u.ETIUserID = wv.CreatedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.PerDiem pd
		WHERE u.ETIUserID = pd.PerDiemLastUpdateETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.RteTemplate rt
		WHERE u.ETIUserID = rt.AuthorID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.BOEApproval ba
		WHERE u.ETIUserID = ba.ApprovalETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.BOEApprovalHistory bah
		WHERE u.ETIUserID = bah.ApprovalETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.BOEComment bc
		WHERE u.ETIUserID = bc.BOECommentETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.BOECommentHistory bch
		WHERE u.ETIUserID = bch.ChangedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.BOEPotentialRole bpr
		WHERE u.ETIUserID = bpr.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.BOEStateHistory bsh
		WHERE u.ETIUserID = bsh.ChangedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.BOEUserRole bur
		WHERE u.ETIUserID = bur.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.BOEUserRoleHistory burh
		WHERE u.ETIUserID = burh.CurrentETIUserID OR u.ETIUserID = burh.UpdatedETIUserID OR u.ETIUserID = burh.ChangedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.SystemUserRole sur
		WHERE u.ETIUserID = sur.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.Trip t
		WHERE u.ETIUserID = t.FareLastUpdateETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.MessageConfirmation mc
		WHERE u.ETIUserID = mc.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.Workspace w
		WHERE u.ETIUserID = w.CostVolumeLeadPricerUserID OR u.ETIUserID = w.CreatedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM dbo.Location l
		WHERE u.ETIUserID = l.UpdatedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.WorkspaceStateHistory wsh
		WHERE u.ETIUserID = wsh.ChangedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.WorkspaceUserRole wur
		WHERE u.ETIUserID = wur.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.PerDiem pd
		WHERE u.ETIUserID = pd.PerDiemLastUpdateETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.RteTemplate rt
		WHERE u.ETIUserID = rt.AuthorID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.BOEApproval ba
		WHERE u.ETIUserID = ba.ApprovalETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.BOEApprovalHistory bah
		WHERE u.ETIUserID = bah.ApprovalETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.BOEComment bc
		WHERE u.ETIUserID = bc.BOECommentETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.BOECommentHistory bch
		WHERE u.ETIUserID = bch.ChangedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.BOEPotentialRole bpr
		WHERE u.ETIUserID = bpr.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.BOEStateHistory bsh
		WHERE u.ETIUserID = bsh.ChangedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.BOEUserRole bur
		WHERE u.ETIUserID = bur.ETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.BOEUserRoleHistory burh
		WHERE u.ETIUserID = burh.CurrentETIUserID OR u.ETIUserID = burh.UpdatedETIUserID OR u.ETIUserID = burh.ChangedByETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.Trip t
		WHERE u.ETIUserID = t.FareLastUpdateETIUserID
	)
	AND NOT EXISTS
	(
		SELECT 1
		FROM version.Workspace w
		WHERE u.ETIUserID = w.CostVolumeLeadPricerUserID OR u.ETIUserID = w.CreatedByETIUserID
	)
)

GO

-- Purge Output Format Templates that are archived and unused
-- Create a temp table to avoid running this query several times
CREATE TABLE #UnusedTableIds (TemplateID INT)

INSERT INTO #UnusedTableIds (TemplateID)
	SELECT o.TemplateID
	FROM [dbo].[OutputFormatTemplate] o
	LEFT JOIN Workspace w ON o.TemplateID = w.TemplateID
	WHERE o.IsActive = 0
	GROUP BY o.TemplateID, o.Template
	HAVING COUNT(w.TemplateID) = 0

-- Remove any templates still in the XREF table
DELETE
FROM [dbo].[OutputFormatTemplateWorkspaceXREF]
WHERE TemplateID in (
	SELECT TemplateID
	FROM #UnusedTableIds
)

-- Clear Parent Template IDs of Templates to be deleted to avoid FK conflicts
UPDATE [dbo].[OutputFormatTemplate]
SET ParentTemplateID = NULL
WHERE TemplateID IN (
	SELECT TemplateID
	FROM #UnusedTableIds
)

-- Finally purge the templates
DELETE FROM [dbo].[OutputFormatTemplate]
WHERE TemplateID IN (  
	SELECT TemplateID
	FROM #UnusedTableIds
) AND TemplateID NOT IN (
	-- Can't delete if an active template has this template listed as its Parent
	SELECT DISTINCT o1.TemplateID 
	FROM [dbo].[OutputFormatTemplate] o1
	JOIN [dbo].[OutputFormatTemplate] o2 ON o1.TemplateID = o2.ParentTemplateID
)

-- Drop the temp table
DROP TABLE #UnusedTableIds

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