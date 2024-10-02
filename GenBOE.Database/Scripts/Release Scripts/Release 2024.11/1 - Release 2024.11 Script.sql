EXEC[dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.11';
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

-- Create temp table of User IDs from all tables using ETIUserId as a FK
CREATE TABLE #TempUserIds(etiUserId INT)

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ETIUserId
FROM dbo.WorkspaceUserXREF

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ChangedByETIUserID
FROM dbo.WorkspaceStateHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ETIUserId
FROM dbo.WorkspaceUserRole

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT CreatedByETIUserID
FROM dbo.WorkspaceVersion

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT PerDiemLastUpdateETIUserID
FROM dbo.PerDiem

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT AuthorID
FROM dbo.RteTemplate

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ApprovalETIUserID
FROM dbo.BOEApproval

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ApprovalETIUserID
FROM dbo.BOEApprovalHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT BOECommentETIUserID
FROM dbo.BOEComment

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ChangedByETIUserID
FROM dbo.BOECommentHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ETIUserId
FROM dbo.BOEPotentialRole

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ChangedByETIUserID
FROM dbo.BOEStateHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ETIUserID
FROM dbo.BOEUserRole

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT CurrentETIUserID
FROM dbo.BOEUserRoleHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT UpdatedETIUserID
FROM dbo.BOEUserRoleHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ChangedByETIUserID
FROM dbo.BOEUserRoleHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ETIUserID
FROM dbo.SystemUserRole

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT FareLastUpdateETIUserID
FROM dbo.Trip

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ETIUserID
FROM dbo.MessageConfirmation

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT CostVolumeLeadPricerUserID
FROM dbo.Workspace

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT CreatedByETIUserID
FROM dbo.Workspace

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT UpdatedByETIUserID
FROM dbo.Location

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ChangedByETIUserID
FROM version.WorkspaceStateHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ETIUserID
FROM version.WorkspaceUserRole

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT PerDiemLastUpdateETIUserID
FROM version.PerDiem

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT AuthorID
FROM version.RteTemplate

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ApprovalETIUserID
FROM version.BOEApproval

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ApprovalETIUserID
FROM version.BOEApprovalHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT BOECommentETIUserID
FROM version.BOEComment

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ChangedByETIUserID
FROM version.BOECommentHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ETIUserID
FROM version.BOEPotentialRole

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ChangedByETIUserID
FROM version.BOEStateHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ETIUserID
FROM version.BOEUserRole

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT CurrentETIUserID
FROM version.BOEUserRoleHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT UpdatedETIUserID
FROM version.BOEUserRoleHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT ChangedByETIUserID
FROM version.BOEUserRoleHistory

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT FareLastUpdateETIUserID
FROM version.Trip

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT CostVolumeLeadPricerUserID
FROM version.Workspace

INSERT INTO #TempUserIds (etiUserId)
SELECT DISTINCT CreatedByETIUserID
FROM version.Workspace

-- Delete users who were not in any of the above tables 1 at a time
CREATE TABLE #TempUserIdsToDelete(etiUserId INT)

INSERT INTO #TempUserIdsToDelete (etiUserId)
SELECT DISTINCT u.ETIUserId
FROM [dbo].[ETIUser] u
	LEFT JOIN #TempUserIds t ON t.etiUserId = u.ETIUserID
	WHERE t.etiUserId is NULL

DECLARE @UserIdToDelete int

WHILE EXISTS (SELECT 1 FROM #TempUserIdsToDelete)
BEGIN
	SELECT TOP 1 @UserIdToDelete = etiUserId FROM #TempUserIdsToDelete

	DELETE FROM [dbo].[ETIUser] WHERE ETIUserId = @UserIdToDelete
	DELETE FROM #TempUserIdsToDelete WHERE etiUserID = @UserIdToDelete
END

 -- Drop the temp tables
DROP TABLE #TempUserIds
DROP TABLE #TempUserIdsToDelete

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