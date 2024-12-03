/*
	## START ##
	2/14/2020 [e374897] - PROPH-1445 CurrentWorkspace
*/

BEGIN
	ALTER TABLE [dbo].[Workspace] ADD [CurrentPTMWorkspace] bit NOT NULL DEFAULT 0;
END

-- 2/7/2024 - e374897 - PROPH-1445 CurrentWorkspace
-- Stored procs changed:
-- upsertWorkspace.sql

/*
   2/14/2020 [e374897] - PROPH-1445 CurrentWorkspace
   ## END ##
*/
