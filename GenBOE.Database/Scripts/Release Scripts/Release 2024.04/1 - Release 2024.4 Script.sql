EXEC [dbo].[UpdateDbVersion] @DbVersion = '1', @AppVersion = '2024.4';
GO

-- 2/7/2024 - e302876 - PROPH-1492-BRC-backend for copy/archive/restore
-- Stored procs changed:
-- copyWorkspace.sql
-- copyWorkspaceVersion.sql
-- createWorkspaceVersion.sql
-- restoreWorkspaceVersion.sql
