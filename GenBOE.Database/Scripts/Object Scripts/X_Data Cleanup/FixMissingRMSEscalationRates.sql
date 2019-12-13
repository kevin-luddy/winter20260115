DEClARE @workspaceId int;
DECLARE myCursor CURSOR FORWARD_ONLY FOR 
SELECT distinct workspaceID
  FROM [dbo].[WorkspaceRMSTravelEscalationRate]
  where MiscRate is NULL or PerDiemRate is NULL;
OPEN myCursor;
FETCH NEXT FROM myCursor INTO @workspaceId;
WHILE @@FETCH_STATUS = 0 BEGIN
    EXECUTE dbo.resetWorkspaceDefaultRates @workspaceId;
    FETCH NEXT FROM myCursor INTO @workspaceId;
END;
CLOSE myCursor;
DEALLOCATE myCursor;